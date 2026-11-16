
namespace SorteFederation.Builders.Services;

public sealed class BuilderMetadataService(BuilderDbContext db)
{
    private static string IncrementPatchVersion(string version)
    {
        // Older migrations cast integer publish counters to text without
        // converting them to semantic versions. Preserve that patch counter.
        if (version is not null && System.Text.RegularExpressions.Regex.IsMatch(version, @"\A[0-9]+\z"))
            version = $"0.0.{System.Numerics.BigInteger.Parse(version, System.Globalization.CultureInfo.InvariantCulture).ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        if (string.IsNullOrWhiteSpace(version) || !System.Text.RegularExpressions.Regex.IsMatch(
            version, @"\A(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\z"))
            throw new InvalidOperationException("Published version must use major.minor.patch, for example 0.0.1.");
        var parts = version.Split('.');
        var patch = System.Numerics.BigInteger.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture) + 1;
        return $"{parts[0]}.{parts[1]}.{patch.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }

    public async Task<BuilderApplication> CreateApplication(CreateApplicationRequest request, CancellationToken token, bool isSystemAdmin = false)
    {
        if (request.VersionNo is null || request.VersionNo.Length > 64 || !System.Text.RegularExpressions.Regex.IsMatch(request.VersionNo, @"\A(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)\z"))
            throw new ArgumentException("Application version must use major.minor.patch, for example 0.0.1.");
        if (request.PublishedVersion is null || request.PublishedVersion.Length > 64 || !System.Text.RegularExpressions.Regex.IsMatch(request.PublishedVersion, @"\A(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)\z"))
            throw new ArgumentException("Initial published version must use major.minor.patch, for example 0.0.1.");

        if (request.TenantId == Guid.Empty || string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Tenant, application key and name are required.");

        await TenantSubscriptionQuota.EnsureCanCreateApplication(db, request.TenantId, token, isSystemAdmin);

        var applicationKey = request.Key.Trim();
        if (await db.Applications.IgnoreQueryFilters().AnyAsync(
                x => x.TenantId == request.TenantId && x.Key == applicationKey, token))
            throw new ApplicationKeyConflictException();

        if (request.Icon is { Length: > 0 } && !System.Text.RegularExpressions.Regex.IsMatch(request.Icon, "^\\['fas','[a-z0-9-]{1,80}'\\]$"))
            throw new ArgumentException("Application icon must be a selected Font Awesome solid icon.");

        var template = request.TemplateId is Guid id
            ? await db.Templates.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.RecordState == RecordState.Active, token)
                ?? throw new KeyNotFoundException("Template was not found.")
            : null;

        if (template is not null)
        {
            var existingApplication = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.TenantId == request.TenantId && x.TemplateId == template.Id &&
                    (x.RecordState == RecordState.Active || x.RecordState == RecordState.Passive))
                .Select(x => new { x.Id, x.Name })
                .FirstOrDefaultAsync(token);
            if (existingApplication is not null)
                throw new TemplateAlreadyUsedException(template.Id, existingApplication.Id, existingApplication.Name);
        }

        // An application must be initialized from the immutable release it records,
        // not from the mutable template listing (which may have advanced later).
        var templateRelease = template is null ? null : await TemplateReleaseService.EnsureReleaseAsync(db, template, token);
        var manifest = templateRelease?.ManifestJson is { Length: > 0 }
            ? TemplateManifestService.Parse(templateRelease.ManifestJson) : null;

        var initialScreen = request.InitialScreen;
        if (initialScreen is not null &&
            (!System.Text.RegularExpressions.Regex.IsMatch(initialScreen.Key, "^[a-z][a-z0-9-]{0,79}$") ||
             string.IsNullOrWhiteSpace(initialScreen.Title) || initialScreen.Title.Length > 120 ||
             initialScreen.HtmlContent is null || initialScreen.HtmlContent.Length > 100_000))
            throw new ArgumentException("The initial screen needs a valid key, title, and HTML under 100 KB.");

        if (initialScreen is not null && manifest?.Screens.Count > 0 && initialScreen.Key != manifest.Screens[0].Key)
            throw new ArgumentException("The initial screen key must match the selected template's first screen.");

        var initialForm = request.InitialForm;

        if (initialForm is not null)
        {
            var keyPattern = "^[A-Za-z][A-Za-z0-9_-]*$";
            if (string.IsNullOrWhiteSpace(initialForm.ObjectName) || string.IsNullOrWhiteSpace(initialForm.FormName) ||
                !System.Text.RegularExpressions.Regex.IsMatch(initialForm.ObjectKey, keyPattern) ||
                !System.Text.RegularExpressions.Regex.IsMatch(initialForm.FormKey, keyPattern) ||
                initialForm.Fields is null || initialForm.Fields.Count > 100 ||
                initialForm.Fields.Any(field => field.Id is not null || string.IsNullOrWhiteSpace(field.Label) ||
                    !System.Text.RegularExpressions.Regex.IsMatch(field.Key, keyPattern) || !Enum.IsDefined(field.Type)) ||
                initialForm.Fields.GroupBy(field => field.Key, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
                throw new ArgumentException("The initial form contains invalid names, keys, types, or too many fields.");

            if (initialForm.LayoutJson is not null) JsonDocument.Parse(initialForm.LayoutJson).Dispose();

            foreach (var field in initialForm.Fields)
                if (field.ConfigurationJson is not null) JsonDocument.Parse(field.ConfigurationJson).Dispose();
        }

        var entity = new BuilderApplication
        {
            TenantId = request.TenantId,
            Key = applicationKey,
            Name = request.Name.Trim(),
            Slugs = request.Slugs?.Select(slug => slug.Trim()).Where(slug => !string.IsNullOrWhiteSpace(slug)).ToArray() ?? Array.Empty<string>(),
            Icon = request.Icon,
            Description = request.Description?.Trim(),
            ApplicationType = request.ApplicationType,
            TemplateId = request.TemplateId,
            TemplateVersion = manifest?.Version,
            TemplateReleaseId = templateRelease?.Id,
            AppliedTemplateVersion = templateRelease?.ReleaseVersion,
            ThemeJson = request.ThemeJson is null ? templateRelease?.ThemeJson : TemplateThemeService.Validate(request.ThemeJson)
        };

        entity.VersionNo = request.VersionNo;
        entity.PublishedVersion = request.PublishedVersion;

        db.Applications.Add(entity);
        if (manifest is not null)
        {
            var objectIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var formIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            foreach (var definition in manifest.Objects)
            {
                var obj = new BuilderObject { TenantId = request.TenantId, ApplicationId = entity.Id, Key = definition.Key, Name = definition.Name };
                objectIds.Add(definition.Key, obj.Id);
                entity.Objects.Add(obj);
            }
            foreach (var definition in manifest.Objects)
            {
                var obj = entity.Objects.Single(item => item.Key == definition.Key);
                foreach (var (field, index) in definition.Fields.Select((field, index) => (field, index)))
                    obj.Fields.Add(new BuilderField
                    {
                        TenantId = request.TenantId,
                        ObjectId = obj.Id,
                        Key = field.Key,
                        Label = field.Label,
                        Type = field.Type,
                        Required = field.Required,
                        Searchable = field.Searchable,
                        SortOrder = index,
                        ConfigurationJson = TemplateManifestService.ResolveFieldConfiguration(field.ConfigurationJson, objectIds)
                    });
                foreach (var form in definition.Forms)
                    obj.Forms.Add(new BuilderForm
                    {
                        TenantId = request.TenantId,
                        ObjectId = obj.Id,
                        Key = form.Key,
                        Name = form.Name,
                        LayoutJson = form.FormConfigJson
                    });
                foreach (var form in obj.Forms) formIds[$"{obj.Key}/{form.Key}"] = form.Id;
            }
            foreach (var relationship in manifest.Relationships ?? Array.Empty<TemplateRelationship>())
                db.Relationships.Add(new BuilderRelationship
                {
                    TenantId = request.TenantId,
                    SourceObjectId = objectIds[relationship.SourceObjectKey],
                    TargetObjectId = objectIds[relationship.TargetObjectKey],
                    Key = relationship.Key,
                    Name = relationship.Name,
                    Cardinality = RelationshipCardinalityRules.Normalize(relationship.Cardinality)
                });
            foreach (var (screen, index) in manifest.Screens.Select((screen, index) => (screen, index)))
            {
                entity.Screens.Add(new BuilderApplicationScreen
                {
                    TenantId = request.TenantId,
                    ApplicationId = entity.Id,
                    Key = screen.Key,
                    Title = index == 0 && initialScreen is not null ? initialScreen.Title.Trim() : screen.Title,
                    HtmlContent = index == 0 && initialScreen is not null ? initialScreen.HtmlContent : screen.HtmlContent,
                    ObjectId = objectIds[screen.ObjectKey],
                    FormKey = screen.FormKey,
                    DefinitionJson = screen.DefinitionJson is null ? null : TemplateManifestService.ClonePage(screen.DefinitionJson, manifest, objectIds, formIds)
                });
                if (manifest.Navigation is null)
                    entity.MenuItems.Add(new BuilderApplicationMenuItem
                    {
                        TenantId = request.TenantId,
                        ApplicationId = entity.Id,
                        Key = screen.Key,
                        Label = index == 0 && initialScreen is not null ? initialScreen.Title.Trim() : screen.Title,
                        Route = $"/builder/applications/{entity.Id}/screens/{screen.Key}",
                        Icon = screen.Icon,
                        SortOrder = index
                    });
            }
            if (manifest.Navigation is not null)
            {
                AddNavigation(manifest.Navigation, null);

                void AddNavigation(IReadOnlyList<TemplateNavigationItem> items, Guid? parentId)
                {
                    for (var index = 0; index < items.Count; index++)
                    {
                        var item = items[index];
                        var screen = item.ScreenKey is null
                            ? null
                            : manifest.Screens.SingleOrDefault(candidate => candidate.Key == item.ScreenKey);
                        var menuItem = new BuilderApplicationMenuItem
                        {
                            TenantId = request.TenantId,
                            ApplicationId = entity.Id,
                            ParentId = parentId,
                            Key = $"template-nav-{item.Key}",
                            Label = item.Label,
                            Route = screen is null ? null : $"/builder/applications/{entity.Id}/screens/{screen.Key}",
                            Icon = item.Icon,
                            SortOrder = index,
                            Visible = true
                        };
                        entity.MenuItems.Add(menuItem);
                        if (item.Children is { Count: > 0 }) AddNavigation(item.Children, menuItem.Id);
                    }
                }
            }
            foreach (var workflow in manifest.Workflows ?? Array.Empty<TemplateWorkflow>())
            {
                ValidateWorkflowDefinition(workflow.DefinitionJson);
                db.Workflows.Add(new BuilderWorkflow
                {
                    TenantId = request.TenantId,
                    ApplicationId = entity.Id,
                    Key = workflow.Key,
                    Name = workflow.Name,
                    DefinitionJson = workflow.DefinitionJson,
                    Status = DefinitionStatus.Draft
                });
            }
        }
        if (initialScreen is not null && manifest?.Screens.Count is not > 0)
        {
            entity.Screens.Add(new BuilderApplicationScreen
            {
                TenantId = request.TenantId,
                ApplicationId = entity.Id,
                Key = initialScreen.Key,
                Title = initialScreen.Title.Trim(),
                HtmlContent = initialScreen.HtmlContent
            });
            entity.MenuItems.Add(new BuilderApplicationMenuItem
            {
                TenantId = request.TenantId,
                ApplicationId = entity.Id,
                Key = initialScreen.Key,
                Label = initialScreen.Title.Trim(),
                Route = $"/builder/applications/{entity.Id}/screens/{initialScreen.Key}",
                SortOrder = 0
            });
        }
        if (initialForm is not null)
        {
            var obj = entity.Objects.FirstOrDefault(item => string.Equals(item.Key, initialForm.ObjectKey, StringComparison.OrdinalIgnoreCase));
            if (obj is null)
            {
                obj = new BuilderObject { TenantId = request.TenantId, ApplicationId = entity.Id, Key = initialForm.ObjectKey.Trim(), Name = initialForm.ObjectName.Trim() };
                entity.Objects.Add(obj);
            }
            else obj.Name = initialForm.ObjectName.Trim();
            obj.Fields.Clear();
            foreach (var (field, index) in initialForm.Fields.Select((field, index) => (field, index)))
                obj.Fields.Add(new BuilderField
                {
                    TenantId = request.TenantId,
                    ObjectId = obj.Id,
                    Key = field.Key.Trim(),
                    Label = field.Label.Trim(),
                    Type = field.Type,
                    SortOrder = index,
                    Required = field.Required,
                    Searchable = field.Searchable,
                    ReadOnly = field.ReadOnly,
                    ConfigurationJson = field.ConfigurationJson
                });
            var form = obj.Forms.FirstOrDefault(item => string.Equals(item.Key, initialForm.FormKey, StringComparison.OrdinalIgnoreCase));
            if (form is null)
            {
                form = new BuilderForm { TenantId = request.TenantId, ObjectId = obj.Id, Key = initialForm.FormKey.Trim(), Name = initialForm.FormName.Trim(), LayoutJson = initialForm.LayoutJson };
                obj.Forms.Add(form);
            }
            else { form.Name = initialForm.FormName.Trim(); form.LayoutJson = initialForm.LayoutJson; }
            if (manifest?.Screens.Count is not > 0)
            {
                var firstScreen = entity.Screens.FirstOrDefault();
                if (firstScreen is not null) { firstScreen.ObjectId = obj.Id; firstScreen.FormKey = form.Key; }
            }
        }
        foreach (var screen in entity.Screens)
            screen.FormId = entity.Objects.SingleOrDefault(obj => obj.Id == screen.ObjectId)?.Forms.SingleOrDefault(form => form.Key == screen.FormKey)?.Id;

        var modules = request.Modules ?? Array.Empty<CreateInitialModuleRequest>();
        if (modules.Count > 50 || modules.Count == 0 && request.Modules is not null || modules.Any(module =>
            string.IsNullOrWhiteSpace(module.Key) || !System.Text.RegularExpressions.Regex.IsMatch(module.Key, "^[a-z][a-z0-9-]{0,79}$") ||
            string.IsNullOrWhiteSpace(module.Name) || module.Name.Length > 120 ||
            !entity.Screens.Any(screen => screen.Key == module.ScreenKey)) ||
            modules.GroupBy(module => module.Key, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
            throw new ArgumentException("Define 1–50 modules with unique keys, names, and a valid application screen.");
        var usesTemplateNavigation = manifest?.Navigation is { Count: > 0 };
        if (modules.Count > 0 && !usesTemplateNavigation) entity.MenuItems.Clear();
        foreach (var (definition, index) in modules.Select((module, index) => (module, index)))
        {
            var screen = entity.Screens.Single(x => x.Key == definition.ScreenKey);
            var dataObject = entity.Objects.SingleOrDefault(x => x.Id == screen.ObjectId);
            var designedForm = dataObject?.Forms.SingleOrDefault(x => x.Key == screen.FormKey);
            var module = new BuilderApplicationModule
            {
                TenantId = request.TenantId,
                ApplicationId = entity.Id,
                Key = definition.Key.Trim(),
                Name = definition.Name.Trim(),
                Description = definition.Description?.Trim(),
                ScreenId = screen.Id,
                ObjectId = dataObject?.Id,
                FormId = designedForm?.Id
            };
            entity.Modules.Add(module);
            if (!usesTemplateNavigation)
            {
                entity.MenuItems.Add(new BuilderApplicationMenuItem
                {
                    TenantId = request.TenantId,
                    ApplicationId = entity.Id,
                    ModuleId = module.Id,
                    Key = definition.Key.Trim(),
                    Label = definition.Name.Trim(),
                    Icon = definition.Icon,
                    Route = $"/builder/applications/{entity.Id}/screens/{definition.ScreenKey}",
                    SortOrder = index,
                    Visible = definition.Visible
                });
            }
        }
        await db.SaveChangesAsync(token);
        return entity;
    }

    public async Task<BuilderObject> CreateObject(CreateObjectRequest request, CancellationToken token)
    {
        if (!await db.Applications.AnyAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token)) throw new KeyNotFoundException("Application was not found for this tenant.");
        var entity = new BuilderObject { TenantId = request.TenantId, ApplicationId = request.ApplicationId, Key = request.Key.Trim(), Name = request.Name.Trim() };
        db.Objects.Add(entity); await db.SaveChangesAsync(token); return entity;
    }

    public async Task<BuilderField> CreateField(CreateFieldRequest request, CancellationToken token)
    {
        if (!await db.Objects.AnyAsync(x => x.Id == request.ObjectId && x.TenantId == request.TenantId, token)) throw new KeyNotFoundException("Object was not found for this tenant.");

        var entity = new BuilderField { TenantId = request.TenantId, ObjectId = request.ObjectId, Key = request.Key.Trim(), Label = request.Label.Trim(), Type = request.Type, SortOrder = request.SortOrder, Required = request.Required, Searchable = request.Searchable, ReadOnly = request.ReadOnly, ConfigurationJson = request.ConfigurationJson };

        db.Fields.Add(entity); await db.SaveChangesAsync(token); return entity;
    }

    public async Task<BuilderForm> CreateForm(CreateFormRequest request, CancellationToken token)
    {
        if (!await db.Objects.AnyAsync(x => x.Id == request.ObjectId && x.TenantId == request.TenantId, token)) throw new KeyNotFoundException("Object was not found for this tenant.");

        var entity = new BuilderForm { TenantId = request.TenantId, ObjectId = request.ObjectId, Key = request.Key.Trim(), Name = request.Name.Trim(), LayoutJson = request.LayoutJson };

        db.Forms.Add(entity); await db.SaveChangesAsync(token); return entity;
    }

    public async Task<BuilderRecord> CreateRecord(CreateRecordRequest request, CancellationToken token)
    {
        if (!await db.Objects.AnyAsync(x => x.Id == request.ObjectId && x.TenantId == request.TenantId && x.Status == DefinitionStatus.Published, token)) throw new InvalidOperationException("Only a published object can accept records.");

        ValidateRecordJson(request.DataJson);
        var entity = new BuilderRecord { TenantId = request.TenantId, ObjectId = request.ObjectId, DataJson = request.DataJson };
        db.Records.Add(entity); await db.SaveChangesAsync(token); return entity;
    }

    public async Task<BuilderRecord?> GetRecord(Guid tenantId, Guid id, CancellationToken token) =>
        await db.Records.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, token);

    public async Task<BuilderRecord> UpdateRecord(Guid tenantId, Guid id, UpdateRecordRequest request, CancellationToken token)
    {
        var record = await db.Records.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, token) ?? throw new KeyNotFoundException("Record was not found.");
        if (record.Version != request.Version) throw new InvalidOperationException("The record was changed by another request.");
        ValidateRecordJson(request.DataJson); record.DataJson = request.DataJson; record.Version++; record.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token); return record;
    }

    public async Task<bool> ArchiveRecord(Guid tenantId, Guid id, CancellationToken token)
    {
        var record = await db.Records.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, token);
        if (record is null) return false;
        db.Records.Remove(record); await db.SaveChangesAsync(token); return true;
    }

    private static void ValidateRecordJson(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson)) throw new ArgumentException("Record data is required.");
        using var document = JsonDocument.Parse(dataJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Record data must be a JSON object.");
    }

    public async Task<IReadOnlyList<string>> ValidateApplication(Guid tenantId, Guid applicationId, CancellationToken token)
    {
        var errors = new List<string>();
        var app = await db.Applications.Include(x => x.Objects).ThenInclude(x => x.Fields).SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);
        if (app is null) return ["Application was not found for this tenant."];
        if (string.IsNullOrWhiteSpace(app.Key)) errors.Add("Application key is required.");
        foreach (var obj in app.Objects)
        {
            if (string.IsNullOrWhiteSpace(obj.Key)) errors.Add($"Object '{obj.Name}' has no key.");
            if (obj.Fields.Count == 0) errors.Add($"Object '{obj.Name}' must contain at least one field.");
            if (obj.Fields.GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1)) errors.Add($"Object '{obj.Name}' contains duplicate field keys.");
        }
        var modules = await db.ApplicationModules.Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId).ToListAsync(token);
        var menus = await db.ApplicationMenuItems.Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId).ToListAsync(token);
        var pages = await db.ApplicationScreens.Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId).ToListAsync(token);
        if (pages.Count == 0) errors.Add("Application must contain at least one page.");
        foreach (var page in pages)
        {
            var hasRenderableContent = page.ObjectId is not null || !string.IsNullOrWhiteSpace(page.HtmlContent);
            if (page.DefinitionJson is not null)
            {
                try
                {
                    var definition = await new PageDefinitionService(db).Validate(tenantId, applicationId, page.DefinitionJson, token, publishing: true);
                    hasRenderableContent |= definition.Children.Count > 0;
                }
                catch (ArgumentException ex) { errors.Add($"Page '{page.Title}': {ex.Message}"); }
            }
            if (!hasRenderableContent)
                errors.Add($"Page '{page.Title}' is empty. Add page elements, bind a data object, or provide page content before publishing.");
            if (page.FormId is Guid formId && !await db.Forms.AnyAsync(x => x.Id == formId && x.TenantId == tenantId && x.ObjectId == page.ObjectId, token))
                errors.Add($"Page '{page.Title}' has an invalid form binding.");
        }
        foreach (var module in modules)
        {
            if (module.ScreenId is not null && !await db.ApplicationScreens.AnyAsync(x => x.Id == module.ScreenId && x.TenantId == tenantId && x.ApplicationId == applicationId, token))
                errors.Add($"Module '{module.Name}' needs an application screen.");
            // Modules are optional organisational groups; pages may contain no form or data source.
        }
        if (menus.Any(x => x.ModuleId != null && !modules.Any(m => m.Id == x.ModuleId))) errors.Add("A menu points to an invalid module.");
        return errors;
    }

    public async Task<object> PublishApplication(PublishApplicationRequest request, CancellationToken token)
    {
        var errors = await ValidateApplication(request.TenantId, request.ApplicationId, token);

        if (errors.Count != 0) throw new InvalidOperationException(string.Join(" ", errors));

        var app = await db.Applications.SingleAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token);

        // New applications start at their declared version. Increment only after
        // at least one immutable published snapshot already exists.
        var hasPublishedVersion = await db.ApplicationVersions.AnyAsync(x =>
            x.TenantId == request.TenantId && x.ApplicationId == app.Id && x.State == GovernanceState.Published, token);
        if (hasPublishedVersion || (app.PublishedVersion is not null &&
            System.Text.RegularExpressions.Regex.IsMatch(app.PublishedVersion, @"\A[0-9]+\z")))
            app.PublishedVersion = IncrementPatchVersion(app.PublishedVersion);
        app.Status = DefinitionStatus.Published;
        app.Version++;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        var objects = await db.Objects.Include(x => x.Fields).Where(x => x.TenantId == request.TenantId && x.ApplicationId == app.Id).ToListAsync(token);

        foreach (var obj in objects) { obj.Status = DefinitionStatus.Published; obj.Version++; obj.UpdatedAt = DateTimeOffset.UtcNow; }

        var objectIds = objects.Select(x => x.Id).ToArray();
        var forms = await db.Forms.Where(x => x.TenantId == request.TenantId && objectIds.Contains(x.ObjectId)).ToListAsync(token);

        foreach (var form in forms) { form.Status = DefinitionStatus.Published; form.Version++; form.UpdatedAt = DateTimeOffset.UtcNow; }
        var modules = await db.ApplicationModules.Where(x => x.TenantId == request.TenantId && x.ApplicationId == app.Id).ToListAsync(token);
        foreach (var module in modules) { module.Status = DefinitionStatus.Published; module.Version++; module.UpdatedAt = DateTimeOffset.UtcNow; }
        var workflows = await db.Workflows.Where(x => x.TenantId == request.TenantId && x.ApplicationId == app.Id).ToListAsync(token);
        foreach (var workflow in workflows) { workflow.Status = DefinitionStatus.Published; workflow.Version++; workflow.UpdatedAt = DateTimeOffset.UtcNow; }
        var menus = await db.ApplicationMenuItems.AsNoTracking().Where(x => x.TenantId == request.TenantId && x.ApplicationId == app.Id).OrderBy(x => x.SortOrder).ToListAsync(token);
        var screens = await db.ApplicationScreens.AsNoTracking().Where(x => x.TenantId == request.TenantId && x.ApplicationId == app.Id).ToListAsync(token);
        var sourceTemplate = app.TemplateId is Guid templateId
            ? await db.Templates.AsNoTracking().Where(x => x.Id == templateId)
                .Select(x => new { x.Id, x.Key, x.Name, x.Category, x.IsBuiltIn }).SingleOrDefaultAsync(token)
            : null;

        var snapshot = JsonSerializer.Serialize(new
        {
            app.Id,
            app.Key,
            app.Name,
            app.PublishedVersion,
            app.ThemeJson,
            app.VersionNo,
            TemplateSource = sourceTemplate is null && app.TemplateId is null ? null : new
            {
                TemplateId = app.TemplateId,
                TemplateKey = sourceTemplate?.Key,
                TemplateName = sourceTemplate?.Name,
                Category = sourceTemplate?.Category,
                IsBuiltIn = sourceTemplate?.IsBuiltIn,
                SourceKind = sourceTemplate is null ? "Unknown" : sourceTemplate.IsBuiltIn ? "Prebuilt" : "DesignStudio",
                ReleaseId = app.TemplateReleaseId,
                ReleaseVersion = app.AppliedTemplateVersion,
                ManifestVersion = app.TemplateVersion
            },
            Modules = modules.Select(m => new { m.Id, m.Key, m.Name, m.Description, m.ScreenId, m.ObjectId, m.FormId }),
            Menus = menus.Select(m => new { m.Id, m.ParentId, m.ModuleId, m.Key, m.Label, m.Route, m.Icon, m.SortOrder, m.Visible, m.PermissionKey }),
            Screens = screens.Select(s => new { s.Id, s.Key, s.Title, s.HtmlContent, s.ObjectId, s.FormKey, s.FormId, s.DefinitionJson }),
            Workflows = workflows.Select(w => new { w.Id, w.Key, w.Name, w.Version, w.DefinitionJson }),
            Forms = forms.Select(f => new { f.Id, f.ObjectId, f.Key, f.Name, f.LayoutJson }),
            Objects = objects.Select(o => new { o.Id, o.Key, o.Name, Fields = o.Fields.Select(f => new { f.Id, f.Key, f.Label, f.Type, f.Required, f.Searchable, f.ReadOnly, f.SortOrder, f.ConfigurationJson }) })
        }, new JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } });

        db.ApplicationVersions.Add(new BuilderApplicationVersion
        {
            TenantId = request.TenantId,
            ApplicationId = app.Id,
            VersionNumber = app.PublishedVersion,
            State = GovernanceState.Published,
            SnapshotJson = snapshot,
            PublishedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(token);
        return new { app.Id, app.PublishedVersion, app.Status };
    }

    public async Task<BuilderApplicationVersion> ApproveVersion(ApproveApplicationVersionRequest request, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        var version = await db.ApplicationVersions.SingleOrDefaultAsync(x => x.Id == request.VersionId && x.TenantId == request.TenantId, token) ?? throw new KeyNotFoundException("Application version was not found.");

        version.State = request.Approve ? GovernanceState.Approved : GovernanceState.Rejected;
        version.ValidationErrorsJson = request.Approve ? null : JsonSerializer.Serialize(new[] { request.Reason ?? "Rejected by approver." });
        version.ApprovedBy = user.Identity?.Name;
        version.ApprovedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);

        return version;
    }

    public async Task<BuilderRelationship> CreateRelationship(CreateRelationshipRequest request, CancellationToken token)
    {
        var objects = await db.Objects.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && (x.Id == request.SourceObjectId || x.Id == request.TargetObjectId))
            .Select(x => new { x.Id, x.ApplicationId }).ToListAsync(token);
        var source = objects.SingleOrDefault(x => x.Id == request.SourceObjectId);
        var target = objects.SingleOrDefault(x => x.Id == request.TargetObjectId);
        if (source is null || target is null) throw new KeyNotFoundException("Both relationship objects must belong to the tenant.");
        if (source.ApplicationId != target.ApplicationId) throw new ArgumentException("Relationship objects must belong to the same application.");
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Relationship key and name are required.");
        var cardinality = RelationshipCardinalityRules.Normalize(request.Cardinality);

        var entity = new BuilderRelationship { TenantId = request.TenantId, SourceObjectId = request.SourceObjectId, TargetObjectId = request.TargetObjectId, Key = request.Key.Trim(), Name = request.Name.Trim(), Cardinality = cardinality };

        db.Relationships.Add(entity); await db.SaveChangesAsync(token); return entity;
    }

    public async Task<IReadOnlyList<BuilderRelationship>> ListRelationships(Guid tenantId, Guid applicationId, CancellationToken token)
    {
        if (!await db.Applications.AsNoTracking().AnyAsync(x => x.Id == applicationId && x.TenantId == tenantId, token))
            throw new KeyNotFoundException("Application was not found for this tenant.");
        var objectIds = db.Objects.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId)
            .Select(x => x.Id);
        return await db.Relationships.AsNoTracking()
            .Where(x => x.TenantId == tenantId && objectIds.Contains(x.SourceObjectId) && objectIds.Contains(x.TargetObjectId))
            .OrderBy(x => x.Name).ToListAsync(token);
    }

    public async Task<BuilderPermission> CreatePermission(CreatePermissionRequest request, CancellationToken token)
    {
        if (!await db.Applications.AnyAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token)) throw new KeyNotFoundException("Application was not found for this tenant.");
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.ResourceType) || string.IsNullOrWhiteSpace(request.Action)) throw new ArgumentException("Permission key, resource type, and action are required.");
        var entity = new BuilderPermission { TenantId = request.TenantId, ApplicationId = request.ApplicationId, Key = request.Key.Trim(), Name = request.Name.Trim(), ResourceType = request.ResourceType.Trim(), Action = request.Action.Trim() };

        db.Permissions.Add(entity); await db.SaveChangesAsync(token); return entity;
    }

    public async Task<BuilderPermissionGrant> GrantPermission(GrantPermissionRequest request, CancellationToken token)
    {
        var permission = await db.Permissions.SingleOrDefaultAsync(x => x.Id == request.PermissionId && x.TenantId == request.TenantId, token)
            ?? throw new KeyNotFoundException("Permission was not found for this tenant.");

        var subjectType = request.SubjectType.Trim().ToLowerInvariant();

        if (subjectType is not ("user" or "role")) throw new ArgumentException("SubjectType must be user or role.");

        if (string.IsNullOrWhiteSpace(request.SubjectId)) throw new ArgumentException("SubjectId is required.");

        var existing = await db.PermissionGrants.SingleOrDefaultAsync(x => x.TenantId == request.TenantId && x.PermissionId == permission.Id && x.SubjectType == subjectType && x.SubjectId == request.SubjectId.Trim(), token);

        if (existing is not null) return existing;
        var grant = new BuilderPermissionGrant { TenantId = request.TenantId, PermissionId = permission.Id, SubjectType = subjectType, SubjectId = request.SubjectId.Trim() };

        db.PermissionGrants.Add(grant);
        await db.SaveChangesAsync(token); return grant;
    }

    public async Task<bool> RevokePermission(Guid tenantId, Guid grantId, CancellationToken token)
    {
        var grant = await db.PermissionGrants.SingleOrDefaultAsync(x => x.Id == grantId && x.TenantId == tenantId, token);
        if (grant is null) return false;
        db.PermissionGrants.Remove(grant);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<BuilderWorkflow> CreateWorkflow(CreateWorkflowRequest request, CancellationToken token)
    {
        if (!await db.Applications.AnyAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token)) throw new KeyNotFoundException("Application was not found for this tenant.");
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Workflow key and name are required.");
        var key = request.Key.Trim().ToLowerInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(key, @"\A[a-z][a-z0-9-]{0,79}\z")) throw new ArgumentException("Workflow key must be a lowercase slug.");
        if (await db.Workflows.AnyAsync(x => x.TenantId == request.TenantId && x.ApplicationId == request.ApplicationId && x.Key == key, token)) throw new InvalidOperationException("A workflow with this key already exists in the application.");
        ValidateWorkflowDefinition(request.DefinitionJson);
        var entity = new BuilderWorkflow { TenantId = request.TenantId, ApplicationId = request.ApplicationId, Key = key, Name = request.Name.Trim(), DefinitionJson = request.DefinitionJson };

        db.Workflows.Add(entity); await db.SaveChangesAsync(token); return entity;
    }

    public Task<List<BuilderWorkflow>> ListWorkflows(Guid tenantId, Guid applicationId, CancellationToken token) =>
        db.Workflows.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId)
            .OrderByDescending(x => x.UpdatedAt).ToListAsync(token);

    public async Task<BuilderWorkflow> UpdateWorkflow(Guid tenantId, Guid workflowId, UpdateWorkflowRequest request, CancellationToken token)
    {
        if (request.TenantId != tenantId) throw new UnauthorizedAccessException("Tenant mismatch.");
        var workflow = await db.Workflows.SingleOrDefaultAsync(x => x.Id == workflowId && x.TenantId == tenantId, token)
            ?? throw new KeyNotFoundException("Workflow was not found.");
        if (workflow.Status != DefinitionStatus.Draft) throw new InvalidOperationException("Only draft workflows can be edited.");
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Workflow name is required.");
        ValidateWorkflowDefinition(request.DefinitionJson);
        workflow.Name = request.Name.Trim();
        workflow.DefinitionJson = request.DefinitionJson;
        workflow.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);
        return workflow;
    }

    public async Task<BuilderConnector> CreateConnector(CreateConnectorRequest request, CancellationToken token)
    {
        if (!await db.Applications.AnyAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token)) throw new KeyNotFoundException("Application was not found for this tenant.");
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Type)) throw new ArgumentException("Connector key and type are required.");
        using (var configuration = JsonDocument.Parse(request.ConfigurationJson))
            if (configuration.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Connector configuration must be a JSON object.");
        var entity = new BuilderConnector { TenantId = request.TenantId, ApplicationId = request.ApplicationId, Key = request.Key.Trim(), Name = request.Name.Trim(), Type = request.Type.Trim(), ConfigurationJson = request.ConfigurationJson };

        db.Connectors.Add(entity); await db.SaveChangesAsync(token); return entity;
    }

    public async Task<bool> HasPermission(Guid tenantId, Guid applicationId, string resourceType, string action, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        if (user.IsInRole(BuilderRoleNames.SystemAdmin)) return true;
        if (user.IsInRole(BuilderRoleNames.TenantAdmin) && user.Claims.Any(x => x.Type == "tenant_id" && x.Value == tenantId.ToString())) return true;
        var permissionIds = await db.Permissions.Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.ResourceType == resourceType && x.Action == action).Select(x => x.Id).ToListAsync(token);
        if (permissionIds.Count == 0) return false;
        var permissionKey = $"{resourceType}:{action}";
        if (user.Claims.Any(x => (x.Type is "builder_permission" or "permission") && (x.Value == permissionKey || x.Value == "*"))) return true;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? user.FindFirstValue("user_id");
        var roles = user.FindAll(ClaimTypes.Role).Concat(user.FindAll("role")).Select(x => x.Value).Distinct().ToArray();
        return await db.PermissionGrants.AnyAsync(x => x.TenantId == tenantId && permissionIds.Contains(x.PermissionId) && ((x.SubjectType == "user" && userId != null && x.SubjectId == userId) || (x.SubjectType == "role" && roles.Contains(x.SubjectId))), token);
    }

    public Task<List<BuilderRecord>> QueryRecords(Guid tenantId, Guid objectId, string? search, CancellationToken token)
    {
        var query = db.Records.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == objectId);

        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.DataJson.Contains(search));

        return query.OrderByDescending(x => x.UpdatedAt).Take(500).ToListAsync(token);
    }

    public async Task<BuilderRecordRelationship> CreateRecordRelationship(CreateRecordRelationshipRequest request, CancellationToken token)
    {
        var relation = await db.Relationships.SingleOrDefaultAsync(x => x.Id == request.RelationshipId && x.TenantId == request.TenantId, token) ?? throw new KeyNotFoundException("Relationship was not found.");
        var source = await db.Records.AnyAsync(x => x.Id == request.SourceRecordId && x.TenantId == request.TenantId && x.ObjectId == relation.SourceObjectId, token);
        var target = await db.Records.AnyAsync(x => x.Id == request.TargetRecordId && x.TenantId == request.TenantId && x.ObjectId == relation.TargetObjectId, token);
        if (!source || !target) throw new InvalidOperationException("Records do not match the relationship objects.");
        var entity = new BuilderRecordRelationship { TenantId = request.TenantId, RelationshipId = request.RelationshipId, SourceRecordId = request.SourceRecordId, TargetRecordId = request.TargetRecordId };
        db.RecordRelationships.Add(entity); await db.SaveChangesAsync(token); return entity;
    }

    public async Task<BuilderWorkflowInstance> StartWorkflow(StartWorkflowRequest request, CancellationToken token)
    {
        if (!await db.Workflows.AnyAsync(x => x.Id == request.WorkflowId && x.TenantId == request.TenantId && x.Status == DefinitionStatus.Published, token)) throw new InvalidOperationException("Only a published workflow can be started.");
        if (!await db.Records.AnyAsync(x => x.Id == request.RecordId && x.TenantId == request.TenantId, token)) throw new KeyNotFoundException("Record was not found.");
        var instance = new BuilderWorkflowInstance { TenantId = request.TenantId, WorkflowId = request.WorkflowId, RecordId = request.RecordId };
        db.WorkflowInstances.Add(instance);
        instance.Tasks.Add(new BuilderWorkflowTask { TenantId = request.TenantId, Name = "Initial task" });
        await db.SaveChangesAsync(token); return instance;
    }

    public async Task<BuilderWorkflow> PublishWorkflow(Guid tenantId, Guid workflowId, CancellationToken token)
    {
        var workflow = await db.Workflows.SingleOrDefaultAsync(x => x.Id == workflowId && x.TenantId == tenantId, token) ?? throw new KeyNotFoundException("Workflow was not found.");
        if (workflow.Status != DefinitionStatus.Draft) throw new InvalidOperationException("Only draft workflows can be published. Reactivate archived workflows instead.");
        ValidateWorkflowDefinition(workflow.DefinitionJson);
        workflow.Status = DefinitionStatus.Published; workflow.Version++; workflow.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token); return workflow;
    }

    public async Task<BuilderWorkflow> DeactivateWorkflow(Guid tenantId, Guid workflowId, CancellationToken token)
    {
        var workflow = await db.Workflows.SingleOrDefaultAsync(x => x.Id == workflowId && x.TenantId == tenantId, token)
            ?? throw new KeyNotFoundException("Workflow was not found.");
        if (workflow.Status != DefinitionStatus.Published) throw new InvalidOperationException("Only active workflows can be deactivated.");
        workflow.Status = DefinitionStatus.Archived;
        workflow.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);
        return workflow;
    }

    public async Task<BuilderWorkflow> ReactivateWorkflow(Guid tenantId, Guid workflowId, CancellationToken token)
    {
        var workflow = await db.Workflows.SingleOrDefaultAsync(x => x.Id == workflowId && x.TenantId == tenantId, token)
            ?? throw new KeyNotFoundException("Workflow was not found.");
        if (workflow.Status != DefinitionStatus.Archived) throw new InvalidOperationException("Only deactivated workflows can be reactivated.");
        workflow.Status = DefinitionStatus.Published;
        workflow.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);
        return workflow;
    }

    private static void ValidateWorkflowDefinition(string definitionJson)
    {
        using var document = JsonDocument.Parse(definitionJson);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Workflow definition must be a JSON object.");
        if (root.TryGetProperty("purchaseApproval", out _) && !PurchaseApprovalDefinition.TryParse(definitionJson, out _))
            throw new ArgumentException("Purchase approval workflow configuration is invalid. Check its request fields and approval tasks.");
        if (!root.TryGetProperty("tasks", out var tasks)) return;
        if (tasks.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Workflow tasks must be an array.");

        var dependencies = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var task in tasks.EnumerateArray())
        {
            var name = task.ValueKind switch
            {
                JsonValueKind.String => task.GetString()?.Trim(),
                JsonValueKind.Object when task.TryGetProperty("name", out var taskName) && taskName.ValueKind == JsonValueKind.String => taskName.GetString()?.Trim(),
                _ => null
            };
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Every workflow task must have a name.");
            if (name.Length > 160)
                throw new ArgumentException("Workflow task names cannot exceed 160 characters.");

            string[] taskDependencies = [];
            if (task.ValueKind == JsonValueKind.Object)
            {
                if (task.TryGetProperty("dependsOn", out var dependsOn) && dependsOn.ValueKind != JsonValueKind.Null)
                {
                    if (dependsOn.ValueKind != JsonValueKind.String)
                        throw new ArgumentException($"Dependencies for '{name}' must be a comma-separated string.");
                    taskDependencies = (dependsOn.GetString() ?? string.Empty).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                }
                if (task.TryGetProperty("maxRetries", out var retries) && (!retries.TryGetInt32(out var retryCount) || retryCount is < 0 or > 20))
                    throw new ArgumentException($"MaxRetries for '{name}' must be between 0 and 20.");
            }
            if (!dependencies.TryAdd(name, taskDependencies))
                throw new ArgumentException($"Workflow task name '{name}' is duplicated.");
        }

        foreach (var (name, taskDependencies) in dependencies)
            foreach (var dependency in taskDependencies)
                if (!dependencies.ContainsKey(dependency) || string.Equals(name, dependency, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"Task '{name}' depends on an unknown task or itself: '{dependency}'.");

        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool HasCycle(string name)
        {
            if (visited.Contains(name)) return false;
            if (!visiting.Add(name)) return true;
            foreach (var dependency in dependencies[name])
                if (HasCycle(dependency)) return true;
            visiting.Remove(name);
            visited.Add(name);
            return false;
        }
        if (dependencies.Keys.Any(HasCycle))
            throw new ArgumentException("Workflow task dependencies cannot contain a cycle.");
    }

    public async Task<bool> CompleteWorkflowTask(CompleteWorkflowTaskRequest request, CancellationToken token)
    {
        var task = await db.WorkflowTasks.Include(x => x.WorkflowInstance).SingleOrDefaultAsync(x => x.Id == request.TaskId && x.TenantId == request.TenantId, token);
        if (task is null) return false;
        if (!task.Completed)
        {
            task.Completed = true;
        }

        // An instance is complete only when every task is complete. This also
        // makes repeated completion requests idempotent and prevents a workflow
        // with multiple tasks from being closed prematurely.
        var remaining = await db.WorkflowTasks.AnyAsync(x =>
            x.WorkflowInstanceId == task.WorkflowInstanceId && !x.Completed, token);
        if (!remaining)
        {
            task.WorkflowInstance.Status = WorkflowInstanceStatus.Completed;
            task.WorkflowInstance.UpdatedAt = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(token); return true;
    }
}
