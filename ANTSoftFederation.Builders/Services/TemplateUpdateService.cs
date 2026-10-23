
namespace SorteFederation.Builders.Services;

public sealed class TemplateUpdateService(BuilderDbContext db)
{
    public async Task<object> ReviewAsync(Guid applicationId, CancellationToken token)
    {
        var app = await db.Applications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == applicationId, token)
            ?? throw new KeyNotFoundException("Application was not found.");
        if (app.TemplateId is not Guid templateId)
            return new { HasTemplate = false, UpdateAvailable = false, Changes = Array.Empty<object>(), Notes = Array.Empty<string>() };

        var latest = await TemplateReleaseService.GetLatestReleaseAsync(
            db.TemplateReleases.AsNoTracking().Where(x => x.TemplateId == templateId), token);
        if (latest is null) return new { HasTemplate = true, UpdateAvailable = false, Changes = Array.Empty<object>(), Notes = new[] { "No published template release is available." } };

        var notes = new List<string>();
        TemplateManifest? oldManifest = null;
        TemplateManifest nextManifest;
        try
        {
            if (latest.ManifestJson is null) throw new ArgumentException("Latest template has no manifest.");
            nextManifest = TemplateManifestService.Parse(latest.ManifestJson);
            if (app.TemplateReleaseId is Guid oldReleaseId)
            {
                var oldRelease = await db.TemplateReleases.AsNoTracking().SingleOrDefaultAsync(x => x.Id == oldReleaseId, token);
                if (oldRelease?.ManifestJson is not null) oldManifest = TemplateManifestService.Parse(oldRelease.ManifestJson);
            }
        }
        catch (ArgumentException ex)
        {
            return new { HasTemplate = true, UpdateAvailable = app.TemplateReleaseId != latest.Id, CurrentVersion = app.AppliedTemplateVersion,
                LatestVersion = latest.ReleaseVersion, ReleaseId = latest.Id, Changes = Array.Empty<object>(), Notes = new[] { $"This release cannot be safely reviewed: {ex.Message}" }, CanApply = false };
        }

        if (app.TemplateReleaseId is null) notes.Add("This application has no recorded source release; review it as an additive baseline.");
        notes.Add("Existing pages, customizations, theme, and business records will not be overwritten or deleted.");
        var existingObjectKeys = await db.Objects.AsNoTracking().Where(x => x.ApplicationId == applicationId).Select(x => x.Key).ToListAsync(token);
        var existingScreenKeys = await db.ApplicationScreens.AsNoTracking().Where(x => x.ApplicationId == applicationId).Select(x => x.Key).ToListAsync(token);
        var existingWorkflowKeys = await db.Workflows.AsNoTracking().Where(x => x.ApplicationId == applicationId).Select(x => x.Key).ToListAsync(token);
        var applicationObjectIds = db.Objects.AsNoTracking().Where(x => x.ApplicationId == applicationId).Select(x => x.Id);
        var existingRelationshipKeys = await db.Relationships.AsNoTracking()
            .Where(x => applicationObjectIds.Contains(x.SourceObjectId) && applicationObjectIds.Contains(x.TargetObjectId))
            .Select(x => x.Key).ToListAsync(token);
        var existingFormKeys = await db.Forms.AsNoTracking().Where(x => x.TenantId == app.TenantId && db.Objects.Any(o => o.Id == x.ObjectId && o.ApplicationId == applicationId))
            .Select(x => x.Key).ToListAsync(token);
        var previousTheme = app.TemplateReleaseId is Guid linkedReleaseId
            ? await db.TemplateReleases.AsNoTracking().Where(x => x.Id == linkedReleaseId).Select(x => x.ThemeJson).FirstOrDefaultAsync(token)
            : null;
        var changes = new List<object>();
        foreach (var obj in nextManifest.Objects)
        {
            var old = oldManifest?.Objects.SingleOrDefault(x => x.Key == obj.Key);
            if (!existingObjectKeys.Contains(obj.Key, StringComparer.OrdinalIgnoreCase))
                changes.Add(new { Kind = "object", Key = obj.Key, Name = obj.Name, Action = "add" });
            else if (old is not null && !string.Equals(old.Name, obj.Name, StringComparison.Ordinal))
                changes.Add(new { Kind = "object", Key = obj.Key, Name = obj.Name, Action = "preserve-existing" });
            foreach (var field in obj.Fields)
            {
                var oldField = old?.Fields.SingleOrDefault(x => x.Key == field.Key);
                if (old?.Fields.All(x => x.Key != field.Key) != false)
                    changes.Add(new { Kind = "field", Key = $"{obj.Key}.{field.Key}", Name = field.Label,
                        Action = existingObjectKeys.Contains(obj.Key, StringComparer.OrdinalIgnoreCase) ? "add-if-missing" : "included-with-object" });
                else if (oldField is not null && oldField != field)
                    changes.Add(new { Kind = "field", Key = $"{obj.Key}.{field.Key}", Name = field.Label, Action = "preserve-existing" });
            }
            foreach (var form in obj.Forms)
            {
                var oldForm = old?.Forms.SingleOrDefault(x => x.Key == form.Key);
                if (old?.Forms.All(x => x.Key != form.Key) != false)
                    changes.Add(new { Kind = "form", Key = $"{obj.Key}/{form.Key}", Name = form.Name,
                        Action = existingObjectKeys.Contains(obj.Key, StringComparer.OrdinalIgnoreCase) && existingFormKeys.Contains(form.Key, StringComparer.OrdinalIgnoreCase) ? "preserve-existing" : "add-if-missing" });
                else if (oldForm is not null && oldForm != form)
                    changes.Add(new { Kind = "form", Key = $"{obj.Key}/{form.Key}", Name = form.Name, Action = "preserve-existing" });
            }
        }
        foreach (var screen in nextManifest.Screens)
        {
            var oldScreen = oldManifest?.Screens.SingleOrDefault(x => x.Key == screen.Key);
            if (oldManifest?.Screens.All(x => x.Key != screen.Key) != false)
                changes.Add(new { Kind = "page", Key = screen.Key, Name = screen.Title,
                    Action = existingScreenKeys.Contains(screen.Key, StringComparer.OrdinalIgnoreCase) ? "preserve-existing" : "add-if-missing" });
            else if (oldScreen is not null && oldScreen != screen)
                changes.Add(new { Kind = "page", Key = screen.Key, Name = screen.Title, Action = "preserve-existing" });
        }
        foreach (var workflow in nextManifest.Workflows ?? Array.Empty<TemplateWorkflow>())
        {
            var oldWorkflow = oldManifest?.Workflows?.SingleOrDefault(x => x.Key == workflow.Key);
            if (oldManifest?.Workflows?.All(x => x.Key != workflow.Key) != false)
                changes.Add(new { Kind = "workflow", Key = workflow.Key, Name = workflow.Name,
                    Action = existingWorkflowKeys.Contains(workflow.Key, StringComparer.OrdinalIgnoreCase) ? "preserve-existing" : "add-if-missing" });
            else if (oldWorkflow is not null && oldWorkflow != workflow)
                changes.Add(new { Kind = "workflow", Key = workflow.Key, Name = workflow.Name, Action = "preserve-existing" });
        }
        foreach (var relationship in nextManifest.Relationships ?? Array.Empty<TemplateRelationship>())
        {
            var oldRelationship = oldManifest?.Relationships?.SingleOrDefault(x => x.Key.Equals(relationship.Key, StringComparison.OrdinalIgnoreCase));
            if (oldManifest?.Relationships?.All(x => !x.Key.Equals(relationship.Key, StringComparison.OrdinalIgnoreCase)) != false)
                changes.Add(new { Kind = "relationship", Key = relationship.Key, Name = relationship.Name,
                    Action = existingRelationshipKeys.Contains(relationship.Key, StringComparer.OrdinalIgnoreCase) ? "preserve-existing" : "add-if-missing" });
            else if (oldRelationship is not null && oldRelationship != relationship)
                changes.Add(new { Kind = "relationship", Key = relationship.Key, Name = relationship.Name, Action = "preserve-existing" });
        }

        if (oldManifest?.Shell is null && nextManifest.Shell is not null)
        {
            var existingDefinitions = await db.ApplicationScreens.AsNoTracking().Where(x => x.ApplicationId == applicationId)
                .Select(x => x.DefinitionJson).ToListAsync(token);
            if (existingDefinitions.Any(IsLegacyAuthoredShell))
                changes.Add(new { Kind = "application shell", Key = "shell", Name = "Application layout", Action = "update-if-unchanged" });
        }

        if (oldManifest is not null && !string.Equals(app.ThemeJson, previousTheme, StringComparison.Ordinal))
            notes.Add("Your theme differs from the source template, so it will be preserved.");
        if (oldManifest is not null && !string.Equals(previousTheme, latest.ThemeJson, StringComparison.Ordinal))
            changes.Add(new { Kind = "theme", Key = "theme", Name = "Application theme", Action = "update-if-unchanged" });
        return new { HasTemplate = true, UpdateAvailable = app.TemplateReleaseId != latest.Id, CurrentVersion = app.AppliedTemplateVersion,
            LatestVersion = latest.ReleaseVersion, ReleaseId = latest.Id, Changes = changes, Notes = notes, CanApply = app.Status == DefinitionStatus.Draft && app.TemplateReleaseId != latest.Id };
    }

    public async Task<object> ApplyAsync(Guid applicationId, Guid tenantId, Guid releaseId, string? actorId, CancellationToken token)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            // Each retry must start from a clean view of the database. The prior
            // attempt may have tracked entities whose transaction was rolled back.
            db.ChangeTracker.Clear();
            return await ApplyTransactionAsync(applicationId, tenantId, releaseId, actorId, token);
        });
    }

    private async Task<object> ApplyTransactionAsync(Guid applicationId, Guid tenantId, Guid releaseId, string? actorId, CancellationToken token)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token)
            ?? throw new KeyNotFoundException("Application was not found for this tenant.");
        if (app.Status != DefinitionStatus.Draft) throw new InvalidOperationException("Template updates can only be applied to draft applications.");
        if (app.TemplateId is not Guid templateId) throw new InvalidOperationException("This application was not created from a starter template.");
        var latest = await db.TemplateReleases.SingleOrDefaultAsync(x => x.Id == releaseId && x.TemplateId == templateId, token)
            ?? throw new KeyNotFoundException("Template release was not found.");
        var currentLatest = await TemplateReleaseService.GetLatestReleaseAsync(
            db.TemplateReleases.Where(x => x.TemplateId == templateId), token);
        if (currentLatest?.Id != releaseId) throw new InvalidOperationException("A newer template release is available. Review it again before applying.");
        if (app.TemplateReleaseId == releaseId) return new { app.Id, Applied = false, Added = 0, Preserved = 0, Version = latest.ReleaseVersion };
        if (latest.ManifestJson is null) throw new InvalidOperationException("This template release has no definition manifest.");

        var manifest = TemplateManifestService.Parse(latest.ManifestJson);
        var previousJson = app.TemplateReleaseId is Guid priorReleaseId
            ? await db.TemplateReleases.AsNoTracking().Where(x => x.Id == priorReleaseId).Select(x => x.ManifestJson).FirstOrDefaultAsync(token)
            : null;
        var previous = previousJson is null ? null : TemplateManifestService.Parse(previousJson);
        var oldRelease = app.TemplateReleaseId is Guid oldReleaseId
            ? await db.TemplateReleases.AsNoTracking().SingleOrDefaultAsync(x => x.Id == oldReleaseId, token) : null;
        var updateTemplateTheme = string.Equals(app.ThemeJson, oldRelease?.ThemeJson, StringComparison.Ordinal);
        var added = 0;
        var preserved = 0;
        var objects = await db.Objects.Include(x => x.Fields).Include(x => x.Forms).Where(x => x.ApplicationId == applicationId).ToListAsync(token);
        var objectIds = objects.ToDictionary(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);
        foreach (var definition in manifest.Objects)
        {
            if (objectIds.ContainsKey(definition.Key)) continue;
            if (previous?.Objects.Any(x => x.Key.Equals(definition.Key, StringComparison.OrdinalIgnoreCase)) == true) continue;
            var obj = new BuilderObject { TenantId = tenantId, ApplicationId = applicationId, Key = definition.Key, Name = definition.Name };
            db.Objects.Add(obj); objectIds[definition.Key] = obj.Id; added++;
        }
        var formIds = objects.SelectMany(x => x.Forms.Select(f => (Key: $"{x.Key}/{f.Key}", f.Id)))
            .ToDictionary(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var screenKeys = await db.ApplicationScreens.Where(x => x.ApplicationId == applicationId).Select(x => x.Key).ToListAsync(token);
        var workflowKeys = await db.Workflows.Where(x => x.ApplicationId == applicationId).Select(x => x.Key).ToListAsync(token);
        var menuKeys = await db.ApplicationMenuItems.Where(x => x.ApplicationId == applicationId).Select(x => x.Key).ToListAsync(token);
        var menuIds = await db.ApplicationMenuItems.Where(x => x.ApplicationId == applicationId)
            .ToDictionaryAsync(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase, token);
        var existingScreens = await db.ApplicationScreens.Where(x => x.ApplicationId == applicationId).ToListAsync(token);
        foreach (var definition in manifest.Objects)
        {
            var previousObject = previous?.Objects.SingleOrDefault(x => x.Key == definition.Key);
            var objectIsNewInRelease = previous is null || previousObject is null;
            if (!objectIds.TryGetValue(definition.Key, out var objectId)) { preserved++; continue; }
            var objectExistsInApplication = objects.Any(x => x.Id == objectId);
            if (!objectExistsInApplication && !objectIsNewInRelease) { preserved++; continue; }
            if (objectExistsInApplication) preserved++;
            var objEntity = objects.SingleOrDefault(x => x.Id == objectId);
            var fieldKeys = objEntity?.Fields.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (field, index) in definition.Fields.Select((x, i) => (x, i)))
            {
                var fieldWasAlreadyInPreviousRelease = previousObject?.Fields.Any(x => x.Key.Equals(field.Key, StringComparison.OrdinalIgnoreCase)) == true;
                if (fieldKeys.Contains(field.Key)) { preserved++; continue; }
                if (!objectIsNewInRelease && fieldWasAlreadyInPreviousRelease) { preserved++; continue; }
                db.Fields.Add(new BuilderField { TenantId = tenantId, ObjectId = objectId, Key = field.Key, Label = field.Label, Type = field.Type,
                    Required = field.Required, Searchable = field.Searchable, SortOrder = index,
                    ConfigurationJson = TemplateManifestService.ResolveFieldConfiguration(field.ConfigurationJson, objectIds) });
                added++;
            }
            foreach (var form in definition.Forms)
            {
                var formKey = $"{definition.Key}/{form.Key}";
                if (formIds.ContainsKey(formKey)) { preserved++; continue; }
                if (!objectIsNewInRelease && previousObject?.Forms.Any(x => x.Key.Equals(form.Key, StringComparison.OrdinalIgnoreCase)) == true) { preserved++; continue; }
                var formEntity = new BuilderForm { TenantId = tenantId, ObjectId = objectId, Key = form.Key, Name = form.Name, LayoutJson = form.FormConfigJson };
                db.Forms.Add(formEntity); formIds[formKey] = formEntity.Id; added++;
            }
        }
        var applicationObjectIds = objects.Select(x => x.Id).Concat(db.ChangeTracker.Entries<BuilderObject>()
            .Where(x => x.State == EntityState.Added && x.Entity.ApplicationId == applicationId).Select(x => x.Entity.Id)).ToHashSet();
        var relationshipKeys = await db.Relationships.Where(x => applicationObjectIds.Contains(x.SourceObjectId) && applicationObjectIds.Contains(x.TargetObjectId))
            .Select(x => x.Key).ToListAsync(token);
        foreach (var relationship in manifest.Relationships ?? Array.Empty<TemplateRelationship>())
        {
            if (relationshipKeys.Contains(relationship.Key, StringComparer.OrdinalIgnoreCase)) { preserved++; continue; }
            var existedInPreviousRelease = previous?.Relationships?.Any(x => x.Key.Equals(relationship.Key, StringComparison.OrdinalIgnoreCase)) == true;
            if (existedInPreviousRelease) { preserved++; continue; }
            db.Relationships.Add(new BuilderRelationship { TenantId = tenantId,
                SourceObjectId = objectIds[relationship.SourceObjectKey], TargetObjectId = objectIds[relationship.TargetObjectKey],
                Key = relationship.Key, Name = relationship.Name,
                Cardinality = RelationshipCardinalityRules.Normalize(relationship.Cardinality) });
            relationshipKeys.Add(relationship.Key);
            added++;
        }
        foreach (var (screen, index) in manifest.Screens.Select((x, i) => (x, i)))
        {
            if (screenKeys.Contains(screen.Key, StringComparer.OrdinalIgnoreCase))
            {
                var existingScreen = existingScreens.SingleOrDefault(x => x.Key.Equals(screen.Key, StringComparison.OrdinalIgnoreCase));
            if (updateTemplateTheme && previous?.Shell is null && manifest.Shell is not null && existingScreen is not null)
                    existingScreen.DefinitionJson = DetachLegacyApplicationShell(existingScreen.DefinitionJson) ?? existingScreen.DefinitionJson;
                preserved++;
                continue;
            }
            if (previous?.Screens.Any(x => x.Key.Equals(screen.Key, StringComparison.OrdinalIgnoreCase)) == true) { preserved++; continue; }
            var screenEntity = new BuilderApplicationScreen { TenantId = tenantId, ApplicationId = applicationId, Key = screen.Key, Title = screen.Title,
                HtmlContent = screen.HtmlContent, ObjectId = objectIds[screen.ObjectKey], FormKey = screen.FormKey,
                FormId = screen.FormKey is null ? null : formIds.GetValueOrDefault($"{screen.ObjectKey}/{screen.FormKey}"),
                DefinitionJson = screen.DefinitionJson is null ? null : TemplateManifestService.ClonePage(screen.DefinitionJson, manifest, objectIds, formIds) };
            db.ApplicationScreens.Add(screenEntity);
            if (manifest.Navigation is null && !menuKeys.Contains(screen.Key, StringComparer.OrdinalIgnoreCase))
                db.ApplicationMenuItems.Add(new BuilderApplicationMenuItem { TenantId = tenantId, ApplicationId = applicationId, Key = screen.Key,
                    Label = screen.Title, Route = $"/builder/applications/{applicationId}/screens/{screen.Key}", Icon = screen.Icon,
                    SortOrder = index, Visible = true });
            added++;
        }
        if (manifest.Navigation is not null)
        {
            AddNavigation(manifest.Navigation, null);

            void AddNavigation(IReadOnlyList<TemplateNavigationItem> items, Guid? parentId)
            {
                for (var index = 0; index < items.Count; index++)
                {
                    var item = items[index];
                    var key = $"template-nav-{item.Key}";
                    if (!menuIds.TryGetValue(key, out var id))
                    {
                        var screen = item.ScreenKey is null ? null : manifest.Screens.SingleOrDefault(x => x.Key == item.ScreenKey);
                        var entity = new BuilderApplicationMenuItem
                        {
                            TenantId = tenantId,
                            ApplicationId = applicationId,
                            ParentId = parentId,
                            Key = key,
                            Label = item.Label,
                            Route = screen is null ? null : $"/builder/applications/{applicationId}/screens/{screen.Key}",
                            Icon = item.Icon,
                            SortOrder = index,
                            Visible = true
                        };
                        db.ApplicationMenuItems.Add(entity);
                        menuIds[key] = entity.Id;
                        menuKeys.Add(key);
                        id = entity.Id;
                        added++;
                    }
                    if (item.Children is { Count: > 0 }) AddNavigation(item.Children, id);
                }
            }
        }
        foreach (var workflow in manifest.Workflows ?? Array.Empty<TemplateWorkflow>())
        {
            if (workflowKeys.Contains(workflow.Key, StringComparer.OrdinalIgnoreCase)) { preserved++; continue; }
            if (previous?.Workflows?.Any(x => x.Key.Equals(workflow.Key, StringComparison.OrdinalIgnoreCase)) == true) { preserved++; continue; }
            db.Workflows.Add(new BuilderWorkflow { TenantId = tenantId, ApplicationId = applicationId, Key = workflow.Key, Name = workflow.Name,
                DefinitionJson = workflow.DefinitionJson, Status = DefinitionStatus.Draft }); added++;
        }
        if (updateTemplateTheme) app.ThemeJson = latest.ThemeJson;

        app.TemplateReleaseId = latest.Id; app.TemplateVersion = manifest.Version; app.AppliedTemplateVersion = latest.ReleaseVersion;

        app.Version++; app.UpdatedAt = DateTimeOffset.UtcNow; app.UpdatedByUserId = actorId;

        db.AuditEvents.Add(new BuilderAuditEvent { TenantId = tenantId, EventType = "StarterTemplateUpdateApplied", ResourceType = "BuilderApplication",
            ResourceId = applicationId, ActorId = actorId,
            DataJson = JsonSerializer.Serialize(new { PreviousVersion = oldRelease?.ReleaseVersion, AppliedVersion = latest.ReleaseVersion, Added = added, Preserved = preserved }) });

        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return new { app.Id, Applied = true, Added = added, Preserved = preserved, Version = latest.ReleaseVersion };
    }

    private static bool IsLegacyAuthoredShell(string? json) => DetachLegacyApplicationShell(json) is not null;

    private static string? DetachLegacyApplicationShell(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var root = System.Text.Json.Nodes.JsonNode.Parse(json)?.AsObject();
            if (root?["children"] is not System.Text.Json.Nodes.JsonArray children) return null;
            foreach (var candidate in children.OfType<System.Text.Json.Nodes.JsonObject>())
            {
                if (candidate["type"]?.GetValue<string>() != "container" ||
                    candidate["children"] is not System.Text.Json.Nodes.JsonArray shellChildren || shellChildren.Count != 2)
                    continue;
                if (shellChildren[0] is not System.Text.Json.Nodes.JsonObject sidebar ||
                    sidebar["type"]?.GetValue<string>() != "container" ||
                    sidebar["children"] is not System.Text.Json.Nodes.JsonArray sidebarChildren ||
                    !sidebarChildren.OfType<System.Text.Json.Nodes.JsonObject>().Any(child => child["type"]?.GetValue<string>() == "heading"))
                    continue;
                if (shellChildren[1] is not System.Text.Json.Nodes.JsonObject main ||
                    main["type"]?.GetValue<string>() != "container" ||
                    main["children"] is not System.Text.Json.Nodes.JsonArray mainChildren || mainChildren.Count != 2 ||
                    mainChildren[0] is not System.Text.Json.Nodes.JsonObject header || header["type"]?.GetValue<string>() != "container" ||
                    mainChildren[1] is not System.Text.Json.Nodes.JsonObject pageContent || pageContent["type"]?.GetValue<string>() != "container")
                    continue;

                var detached = (System.Text.Json.Nodes.JsonObject)root.DeepClone();
                detached["children"] = new System.Text.Json.Nodes.JsonArray(pageContent.DeepClone());
                return detached.ToJsonString();
            }
        }
        catch (System.Text.Json.JsonException) { return null; }
        catch (InvalidOperationException) { return null; }
        return null;
    }
}
