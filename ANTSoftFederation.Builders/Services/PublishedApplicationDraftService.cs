using SorteFederation.Builders.Data.Enums;
using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Services;

public sealed class PublishedApplicationDraftService(BuilderDbContext db, BuilderMetadataService metadata)
{
    private const string ForkPrefix = "published-draft-fork|";
    private static readonly JsonSerializerOptions SnapshotOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<bool> IsPublishedForkDraftAsync(Guid applicationId, Guid tenantId, CancellationToken token) =>
        db.ApplicationVersions.AsNoTracking().AnyAsync(x => x.ApplicationId == applicationId && x.TenantId == tenantId &&
            x.State == GovernanceState.Draft && x.VersionNumber.StartsWith(ForkPrefix), token);

    public async Task<object> CreateDraftAsync(Guid sourceApplicationId, string? actorId, CancellationToken token)
    {
        var source = await db.Applications.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == sourceApplicationId, token)
            ?? throw new KeyNotFoundException("Application was not found.");
        if (source.Status != DefinitionStatus.Published || source.RecordState is not (RecordState.Active or RecordState.Passive))
            throw new InvalidOperationException("Only an active or passive published application can be copied to a draft.");

        var sourceVersion = await db.ApplicationVersions.AsNoTracking().SingleOrDefaultAsync(x =>
            x.ApplicationId == source.Id && x.TenantId == source.TenantId && x.VersionNumber == source.PublishedVersion &&
            x.State == GovernanceState.Published, token)
            ?? throw new InvalidOperationException("The current published snapshot is missing; a safe draft cannot be created.");
        var marker = $"{ForkPrefix}{source.Id:N}|{source.PublishedVersion}";
        var existingFork = await db.ApplicationVersions.AsNoTracking().Where(x => x.TenantId == source.TenantId &&
                x.VersionNumber == marker && x.State == GovernanceState.Draft)
            .Select(x => x.ApplicationId).FirstOrDefaultAsync(token);
        if (existingFork != Guid.Empty)
        {
            var existingDraft = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == existingFork && x.TenantId == source.TenantId &&
                    x.Status == DefinitionStatus.Draft && x.RecordState == RecordState.Active, token);
            if (existingDraft is not null)
                return new { id = existingDraft.Id, existingDraft.TenantId, existingDraft.Key, existingDraft.Name, ExistingDraft = true };
        }

        var snapshot = JsonSerializer.Deserialize<PublishedSnapshot>(sourceVersion.SnapshotJson, SnapshotOptions)
            ?? throw new InvalidOperationException("The published snapshot could not be read.");
        var templateSource = snapshot.TemplateSource;
        var draft = new BuilderApplication
        {
            TenantId = source.TenantId,
            Key = await CreateUniqueDraftKeyAsync(source.TenantId, source.Key, token),
            Name = $"{source.Name} (Draft)",
            Icon = source.Icon,
            Description = source.Description,
            ApplicationType = source.ApplicationType,
            TemplateId = templateSource?.TemplateId ?? source.TemplateId,
            TemplateVersion = templateSource?.ManifestVersion ?? source.TemplateVersion,
            TemplateReleaseId = templateSource?.ReleaseId ?? source.TemplateReleaseId,
            AppliedTemplateVersion = templateSource?.ReleaseVersion ?? source.AppliedTemplateVersion,
            ThemeJson = snapshot.ThemeJson,
            VersionNo = source.VersionNo,
            PublishedVersion = source.PublishedVersion,
            Slugs = []
        };

        var ids = BuildIdMap(snapshot, draft.Id);
        draft.ThemeJson = RewriteIds(snapshot.ThemeJson, ids);
        AddSnapshotContent(draft, snapshot, ids);
        db.Applications.Add(draft);
        db.ApplicationVersions.Add(new BuilderApplicationVersion
        {
            TenantId = source.TenantId,
            ApplicationId = draft.Id,
            VersionNumber = marker,
            State = GovernanceState.Draft,
            SnapshotJson = JsonSerializer.Serialize(new { SourceApplicationId = source.Id, SourceVersion = source.PublishedVersion })
        });
        db.AuditEvents.Add(new BuilderAuditEvent
        {
            TenantId = source.TenantId,
            EventType = "PublishedApplicationDraftCreated",
            ResourceType = "BuilderApplication",
            ResourceId = draft.Id,
            ActorId = actorId,
            DataJson = JsonSerializer.Serialize(new { SourceApplicationId = source.Id, SourceVersion = source.PublishedVersion })
        });
        await db.SaveChangesAsync(token);
        return new { id = draft.Id, draft.TenantId, draft.Key, draft.Name, ExistingDraft = false };
    }

    public async Task<object> PublishDraftAsync(Guid draftApplicationId, Guid tenantId, string? actorId, CancellationToken token)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        var draft = await db.Applications.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == draftApplicationId &&
            x.TenantId == tenantId && x.Status == DefinitionStatus.Draft && x.RecordState == RecordState.Active, token)
            ?? throw new KeyNotFoundException("Draft application was not found.");
        var marker = await db.ApplicationVersions.Where(x => x.ApplicationId == draft.Id && x.TenantId == tenantId &&
                x.State == GovernanceState.Draft && x.VersionNumber.StartsWith(ForkPrefix))
            .Select(x => x.VersionNumber).SingleOrDefaultAsync(token)
            ?? throw new InvalidOperationException("This is not a draft copied from a published application.");
        var (sourceId, sourceVersion) = ParseForkMarker(marker);
        var source = await db.Applications.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == sourceId && x.TenantId == tenantId, token)
            ?? throw new KeyNotFoundException("The original published application no longer exists.");
        if (source.Status != DefinitionStatus.Published || source.RecordState is not (RecordState.Active or RecordState.Passive))
            throw new InvalidOperationException("The original application is no longer published and available.");
        if (!string.Equals(source.PublishedVersion, sourceVersion, StringComparison.Ordinal))
            throw new InvalidOperationException("The original application has a newer published version. Create a fresh draft before publishing changes.");

        var errors = await metadata.ValidateApplication(tenantId, draft.Id, token);
        if (errors.Count != 0) throw new InvalidOperationException(string.Join(" ", errors));

        await MergeDraftIntoPublishedAsync(draft, source, actorId, token);
        await db.SaveChangesAsync(token);
        await metadata.PublishApplication(new PublishApplicationRequest(tenantId, source.Id), token);

        draft.Status = DefinitionStatus.Archived;
        draft.RecordState = RecordState.Deleted;
        draft.UpdatedByUserId = actorId;
        draft.UpdatedAt = DateTimeOffset.UtcNow;
        db.AuditEvents.Add(new BuilderAuditEvent
        {
            TenantId = tenantId,
            EventType = "PublishedApplicationDraftPromoted",
            ResourceType = "BuilderApplication",
            ResourceId = source.Id,
            ActorId = actorId,
            DataJson = JsonSerializer.Serialize(new { DraftApplicationId = draft.Id, SourceVersion = sourceVersion, PublishedVersion = source.PublishedVersion })
        });
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return new { promotedApplicationId = source.Id, source.Name, source.PublishedVersion, status = source.Status.ToString() };
    }

    private async Task MergeDraftIntoPublishedAsync(BuilderApplication draft, BuilderApplication source, string? actorId, CancellationToken token)
    {
        var idMap = new Dictionary<Guid, Guid> { [draft.Id] = source.Id };
        var draftScreens = await db.ApplicationScreens.Where(x => x.TenantId == draft.TenantId && x.ApplicationId == draft.Id).ToListAsync(token);
        var draftModules = await db.ApplicationModules.Where(x => x.TenantId == draft.TenantId && x.ApplicationId == draft.Id).ToListAsync(token);
        var draftMenus = await db.ApplicationMenuItems.Where(x => x.TenantId == draft.TenantId && x.ApplicationId == draft.Id).ToListAsync(token);
        // Remove the draft's dependants before reusing its published design rows.
        // The surrounding transaction restores them if any later validation fails.
        db.ApplicationMenuItems.RemoveRange(draftMenus);
        db.ApplicationModules.RemoveRange(draftModules);
        db.ApplicationScreens.RemoveRange(draftScreens);
        await db.SaveChangesAsync(token);

        var sourceObjects = await db.Objects.Include(x => x.Fields).Include(x => x.Forms)
            .Where(x => x.TenantId == source.TenantId && x.ApplicationId == source.Id).ToListAsync(token);
        var draftObjects = await db.Objects.Include(x => x.Fields).Include(x => x.Forms)
            .Where(x => x.TenantId == draft.TenantId && x.ApplicationId == draft.Id).ToListAsync(token);

        foreach (var draftObject in draftObjects)
        {
            var target = sourceObjects.SingleOrDefault(x => x.Key == draftObject.Key);
            if (target is null)
            {
                draftObject.ApplicationId = source.Id;
                draftObject.UpdatedAt = DateTimeOffset.UtcNow;
                sourceObjects.Add(draftObject);
                target = draftObject;
            }
            else
            {
                target.Name = draftObject.Name;
                target.UpdatedAt = DateTimeOffset.UtcNow;
                foreach (var draftField in draftObject.Fields.ToList())
                {
                    var targetField = target.Fields.SingleOrDefault(x => x.Key == draftField.Key);
                    if (targetField is null)
                    {
                        draftField.ObjectId = target.Id;
                        target.Fields.Add(draftField);
                        draftObject.Fields.Remove(draftField);
                    }
                    else
                    {
                        CopyField(draftField, targetField);
                        idMap[draftField.Id] = targetField.Id;
                        draftObject.Fields.Remove(draftField);
                        db.Fields.Remove(draftField);
                    }
                }
                foreach (var draftForm in draftObject.Forms.ToList())
                {
                    var targetForm = target.Forms.SingleOrDefault(x => x.Key == draftForm.Key);
                    if (targetForm is null)
                    {
                        draftForm.ObjectId = target.Id;
                        target.Forms.Add(draftForm);
                        draftObject.Forms.Remove(draftForm);
                    }
                    else
                    {
                        targetForm.Name = draftForm.Name;
                        targetForm.LayoutJson = draftForm.LayoutJson;
                        targetForm.UpdatedAt = DateTimeOffset.UtcNow;
                        idMap[draftForm.Id] = targetForm.Id;
                        draftObject.Forms.Remove(draftForm);
                        db.Forms.Remove(draftForm);
                    }
                }
                draftObject.Fields.Clear();
                draftObject.Forms.Clear();
                db.Objects.Remove(draftObject);
            }
            idMap[draftObject.Id] = target.Id;
        }

        foreach (var item in sourceObjects)
        {
            foreach (var field in item.Fields) field.ConfigurationJson = RewriteIds(field.ConfigurationJson, idMap);
            foreach (var form in item.Forms) form.LayoutJson = RewriteIds(form.LayoutJson, idMap);
        }

        var sourceScreens = await db.ApplicationScreens.Where(x => x.TenantId == source.TenantId && x.ApplicationId == source.Id).ToListAsync(token);
        foreach (var draftScreen in draftScreens)
        {
            var target = sourceScreens.SingleOrDefault(x => x.Key == draftScreen.Key);
            if (target is null)
            {
                target = draftScreen;
                target.ApplicationId = source.Id;
                db.Entry(target).State = EntityState.Added;
                sourceScreens.Add(target);
            }
            else
            {
                target.Title = draftScreen.Title;
                target.HtmlContent = draftScreen.HtmlContent;
                target.ObjectId = draftScreen.ObjectId is Guid objectId ? idMap.GetValueOrDefault(objectId, objectId) : null;
                target.FormId = draftScreen.FormId is Guid formId ? idMap.GetValueOrDefault(formId, formId) : null;
                target.FormKey = draftScreen.FormKey;
                target.DefinitionJson = draftScreen.DefinitionJson;
                target.UpdatedAt = DateTimeOffset.UtcNow;
                idMap[draftScreen.Id] = target.Id;
            }
            target.ObjectId = target.ObjectId is Guid oid ? idMap.GetValueOrDefault(oid, oid) : null;
            target.FormId = target.FormId is Guid fid ? idMap.GetValueOrDefault(fid, fid) : null;
            target.HtmlContent = RewriteIds(target.HtmlContent, idMap) ?? "";
            target.DefinitionJson = RewriteIds(target.DefinitionJson, idMap);
            idMap[draftScreen.Id] = target.Id;
        }

        var sourceModules = await db.ApplicationModules.Where(x => x.TenantId == source.TenantId && x.ApplicationId == source.Id).ToListAsync(token);
        foreach (var draftModule in draftModules)
        {
            var target = sourceModules.SingleOrDefault(x => x.Key == draftModule.Key);
            if (target is null)
            {
                target = draftModule;
                target.ApplicationId = source.Id;
                db.Entry(target).State = EntityState.Added;
                sourceModules.Add(target);
            }
            else
            {
                target.Name = draftModule.Name;
                target.Description = draftModule.Description;
                target.ScreenId = draftModule.ScreenId is Guid screenId ? idMap.GetValueOrDefault(screenId, screenId) : null;
                target.ObjectId = draftModule.ObjectId is Guid objectId ? idMap.GetValueOrDefault(objectId, objectId) : null;
                target.FormId = draftModule.FormId is Guid formId ? idMap.GetValueOrDefault(formId, formId) : null;
                target.UpdatedAt = DateTimeOffset.UtcNow;
                idMap[draftModule.Id] = target.Id;
            }
            target.ScreenId = target.ScreenId is Guid sid ? idMap.GetValueOrDefault(sid, sid) : null;
            target.ObjectId = target.ObjectId is Guid oid ? idMap.GetValueOrDefault(oid, oid) : null;
            target.FormId = target.FormId is Guid fid ? idMap.GetValueOrDefault(fid, fid) : null;
            idMap[draftModule.Id] = target.Id;
        }

        var sourceMenus = await db.ApplicationMenuItems.Where(x => x.TenantId == source.TenantId && x.ApplicationId == source.Id).ToListAsync(token);
        var menuTargets = new Dictionary<Guid, BuilderApplicationMenuItem>();
        foreach (var draftMenu in draftMenus)
        {
            var target = sourceMenus.SingleOrDefault(x => x.Key == draftMenu.Key);
            if (target is null)
            {
                target = draftMenu;
                target.ApplicationId = source.Id;
                db.Entry(target).State = EntityState.Added;
                sourceMenus.Add(target);
            }
            target.Label = draftMenu.Label;
            target.Route = RewriteIds(draftMenu.Route, idMap);
            target.Icon = draftMenu.Icon;
            target.SortOrder = draftMenu.SortOrder;
            target.Visible = draftMenu.Visible;
            target.PermissionKey = draftMenu.PermissionKey;
            target.ModuleId = draftMenu.ModuleId is Guid moduleId ? idMap.GetValueOrDefault(moduleId, moduleId) : null;
            target.UpdatedAt = DateTimeOffset.UtcNow;
            menuTargets[draftMenu.Id] = target;
            idMap[draftMenu.Id] = target.Id;
        }
        foreach (var draftMenu in draftMenus)
            menuTargets[draftMenu.Id].ParentId = draftMenu.ParentId is Guid parentId && menuTargets.TryGetValue(parentId, out var parent)
                ? parent.Id : null;

        foreach (var item in sourceScreens)
        {
            item.HtmlContent = RewriteIds(item.HtmlContent, idMap) ?? "";
            item.DefinitionJson = RewriteIds(item.DefinitionJson, idMap);
        }
        foreach (var item in sourceMenus) item.Route = RewriteIds(item.Route, idMap);

        source.ThemeJson = RewriteIds(draft.ThemeJson, idMap);
        source.TemplateId = draft.TemplateId;
        source.TemplateVersion = draft.TemplateVersion;
        source.TemplateReleaseId = draft.TemplateReleaseId;
        source.AppliedTemplateVersion = draft.AppliedTemplateVersion;
        source.UpdatedAt = DateTimeOffset.UtcNow;
        source.UpdatedByUserId = actorId;
    }

    private static void CopyField(BuilderField from, BuilderField to)
    {
        to.Label = from.Label;
        to.Type = from.Type;
        to.SortOrder = from.SortOrder;
        to.Required = from.Required;
        to.Searchable = from.Searchable;
        to.ReadOnly = from.ReadOnly;
        to.ConfigurationJson = from.ConfigurationJson;
        to.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private async Task<string> CreateUniqueDraftKeyAsync(Guid tenantId, string sourceKey, CancellationToken token)
    {
        var prefix = sourceKey[..Math.Min(sourceKey.Length, 64)];
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = $"-draft-{Guid.NewGuid():N}"[..15];
            var candidate = $"{prefix[..Math.Min(prefix.Length, 80 - suffix.Length)]}{suffix}";
            if (!await db.Applications.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Key == candidate, token)) return candidate;
        }
        throw new InvalidOperationException("A unique draft application key could not be generated.");
    }

    private static (Guid SourceId, string SourceVersion) ParseForkMarker(string marker)
    {
        var parts = marker.Split('|', 3);
        if (parts.Length != 3 || !string.Equals(parts[0], "published-draft-fork", StringComparison.Ordinal) ||
            !Guid.TryParseExact(parts[1], "N", out var sourceId) || string.IsNullOrWhiteSpace(parts[2]))
            throw new InvalidOperationException("The draft's published source link is invalid.");
        return (sourceId, parts[2]);
    }

    private static Dictionary<Guid, Guid> BuildIdMap(PublishedSnapshot snapshot, Guid newApplicationId)
    {
        var map = new Dictionary<Guid, Guid> { [snapshot.Id] = newApplicationId };
        void Add(Guid id) { if (id != Guid.Empty && !map.ContainsKey(id)) map[id] = Guid.CreateVersion7(); }
        foreach (var item in snapshot.Objects) { Add(item.Id); foreach (var field in item.Fields) Add(field.Id); }
        foreach (var item in snapshot.Forms) Add(item.Id);
        foreach (var item in snapshot.Screens) Add(item.Id);
        foreach (var item in snapshot.Modules) Add(item.Id);
        foreach (var item in snapshot.Menus) Add(item.Id);
        return map;
    }

    private static Guid Map(Guid id, IReadOnlyDictionary<Guid, Guid> map) => map.GetValueOrDefault(id, id);
    private static Guid? Map(Guid? id, IReadOnlyDictionary<Guid, Guid> map) => id is Guid value ? Map(value, map) : null;

    private static string? RewriteIds(string? value, IReadOnlyDictionary<Guid, Guid> map)
    {
        if (value is null || map.Count == 0) return value;
        foreach (var pair in map) value = value.Replace(pair.Key.ToString(), pair.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        return value;
    }

    private static void AddSnapshotContent(BuilderApplication draft, PublishedSnapshot snapshot, IReadOnlyDictionary<Guid, Guid> ids)
    {
        foreach (var item in snapshot.Objects)
        {
            var obj = new BuilderObject { Id = Map(item.Id, ids), TenantId = draft.TenantId, ApplicationId = draft.Id, Key = item.Key, Name = item.Name };
            foreach (var field in item.Fields)
                obj.Fields.Add(new BuilderField { Id = Map(field.Id, ids), TenantId = draft.TenantId, ObjectId = obj.Id, Key = field.Key,
                    Label = field.Label, Type = field.Type, SortOrder = field.SortOrder, Required = field.Required, Searchable = field.Searchable,
                    ReadOnly = field.ReadOnly, ConfigurationJson = RewriteIds(field.ConfigurationJson, ids) });
            foreach (var form in snapshot.Forms.Where(x => x.ObjectId == item.Id))
                obj.Forms.Add(new BuilderForm { Id = Map(form.Id, ids), TenantId = draft.TenantId, ObjectId = obj.Id, Key = form.Key,
                    Name = form.Name, LayoutJson = RewriteIds(form.LayoutJson, ids) });
            draft.Objects.Add(obj);
        }
        foreach (var screen in snapshot.Screens)
            draft.Screens.Add(new BuilderApplicationScreen { Id = Map(screen.Id, ids), TenantId = draft.TenantId, ApplicationId = draft.Id,
                Key = screen.Key, Title = screen.Title, HtmlContent = RewriteIds(screen.HtmlContent, ids) ?? "", ObjectId = Map(screen.ObjectId, ids),
                FormKey = screen.FormKey, FormId = Map(screen.FormId, ids), DefinitionJson = RewriteIds(screen.DefinitionJson, ids) });
        foreach (var module in snapshot.Modules)
            draft.Modules.Add(new BuilderApplicationModule { Id = Map(module.Id, ids), TenantId = draft.TenantId, ApplicationId = draft.Id,
                Key = module.Key, Name = module.Name, Description = module.Description, ScreenId = Map(module.ScreenId, ids),
                ObjectId = Map(module.ObjectId, ids), FormId = Map(module.FormId, ids) });
        var menuMap = snapshot.Menus.ToDictionary(x => x.Id, x => Map(x.Id, ids));
        foreach (var menu in snapshot.Menus)
            draft.MenuItems.Add(new BuilderApplicationMenuItem { Id = menuMap[menu.Id], TenantId = draft.TenantId, ApplicationId = draft.Id,
                ParentId = menu.ParentId is Guid parent ? menuMap.GetValueOrDefault(parent) : null,
                ModuleId = Map(menu.ModuleId, ids), Key = menu.Key, Label = menu.Label, Route = RewriteIds(menu.Route, ids),
                Icon = menu.Icon, SortOrder = menu.SortOrder, Visible = menu.Visible, PermissionKey = menu.PermissionKey });
    }

    private sealed class PublishedSnapshot
    {
        public Guid Id { get; set; }
        public string? ThemeJson { get; set; }
        public TemplateSourceSnapshot? TemplateSource { get; set; }
        public List<SnapshotModule> Modules { get; set; } = [];
        public List<SnapshotMenu> Menus { get; set; } = [];
        public List<SnapshotScreen> Screens { get; set; } = [];
        public List<SnapshotForm> Forms { get; set; } = [];
        public List<SnapshotObject> Objects { get; set; } = [];
    }

    private sealed class TemplateSourceSnapshot
    {
        public Guid? TemplateId { get; set; }
        public Guid? ReleaseId { get; set; }
        public string? ReleaseVersion { get; set; }
        public int? ManifestVersion { get; set; }
    }

    private sealed class SnapshotModule { public Guid Id { get; set; } public Guid? ScreenId { get; set; } public Guid? ObjectId { get; set; } public Guid? FormId { get; set; } public string Key { get; set; } = ""; public string Name { get; set; } = ""; public string? Description { get; set; } }
    private sealed class SnapshotMenu { public Guid Id { get; set; } public Guid? ParentId { get; set; } public Guid? ModuleId { get; set; } public string Key { get; set; } = ""; public string Label { get; set; } = ""; public string? Route { get; set; } public string? Icon { get; set; } public int SortOrder { get; set; } public bool Visible { get; set; } public string? PermissionKey { get; set; } }
    private sealed class SnapshotScreen { public Guid Id { get; set; } public string Key { get; set; } = ""; public string Title { get; set; } = ""; public string HtmlContent { get; set; } = ""; public Guid? ObjectId { get; set; } public string? FormKey { get; set; } public Guid? FormId { get; set; } public string? DefinitionJson { get; set; } }
    private sealed class SnapshotForm { public Guid Id { get; set; } public Guid ObjectId { get; set; } public string Key { get; set; } = ""; public string Name { get; set; } = ""; public string? LayoutJson { get; set; } }
    private sealed class SnapshotObject { public Guid Id { get; set; } public string Key { get; set; } = ""; public string Name { get; set; } = ""; public List<SnapshotField> Fields { get; set; } = []; }
    private sealed class SnapshotField { public Guid Id { get; set; } public string Key { get; set; } = ""; public string Label { get; set; } = ""; public BuilderFieldType Type { get; set; } public int SortOrder { get; set; } public bool Required { get; set; } public bool Searchable { get; set; } public bool ReadOnly { get; set; } public string? ConfigurationJson { get; set; } }
}
