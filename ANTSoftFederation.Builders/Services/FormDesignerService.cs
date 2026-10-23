using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Services;

public sealed class FormBlockNotFoundException(string message) : InvalidOperationException(message);

public sealed class FormDesignerService(BuilderDbContext db)
{
    public async Task<DesignerApplicationResponse?> GetApplication(Guid tenantId, Guid applicationId, CancellationToken token)
    {
        var application = await db.Applications.AsNoTracking().Include(x => x.Template).SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);

        if (application is null) return null;

        var objects = await db.Objects.AsNoTracking().Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId).OrderBy(x => x.Name).ToListAsync(token);

        var ids = objects.Select(x => x.Id).ToArray();

        var fields = await db.Fields.AsNoTracking().Where(x => x.TenantId == tenantId && ids.Contains(x.ObjectId)).OrderBy(x => x.SortOrder).ToListAsync(token);

        var forms = await db.Forms.AsNoTracking().Where(x => x.TenantId == tenantId && ids.Contains(x.ObjectId)).OrderBy(x => x.CreatedAt).ToListAsync(token);

        var modules = await db.ApplicationModules.AsNoTracking().Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId).OrderBy(x => x.Name).ToListAsync(token);

        return new DesignerApplicationResponse(application.Id, application.TenantId, application.Key, application.Name, application.Status.ToString(),
            objects.Select(obj =>
            {
                var form = forms.FirstOrDefault(x => x.ObjectId == obj.Id);
                var objectForms = forms.Where(x => x.ObjectId == obj.Id).ToArray();
                return new DesignerObjectResponse(obj.Id, obj.Key, obj.Name, obj.Status.ToString(), form?.Id, form?.Key, form?.Name, form?.LayoutJson,
                    fields.Where(x => x.ObjectId == obj.Id).Select(x => new DesignerFieldResponse(x.Id, x.Key, x.Label, x.Type, x.SortOrder, x.Required, x.Searchable, x.ReadOnly, x.ConfigurationJson)).ToArray(),
                    objectForms.Select(x => new DesignerFormResponse(x.Id, x.Key, x.Name, x.LayoutJson)).ToArray());
            }).ToArray(), application.TemplateId, application.Template?.HtmlContent, application.Template?.ScssContent,
            modules.Select(module => {
                var obj = objects.SingleOrDefault(x => x.Id == module.ObjectId);
                var form = forms.SingleOrDefault(x => x.Id == module.FormId);
                return new DesignerModuleResponse(module.Id, module.Key, module.Name, module.ScreenId, module.ObjectId, module.FormId,
                    obj is null ? null : new DesignerObjectResponse(obj.Id, obj.Key, obj.Name, obj.Status.ToString(), form?.Id, form?.Key, form?.Name, form?.LayoutJson,
                        fields.Where(x => x.ObjectId == obj.Id).Select(x => new DesignerFieldResponse(x.Id, x.Key, x.Label, x.Type, x.SortOrder, x.Required, x.Searchable, x.ReadOnly, x.ConfigurationJson)).ToArray(),
                        forms.Where(x => x.ObjectId == obj.Id).Select(x => new DesignerFormResponse(x.Id, x.Key, x.Name, x.LayoutJson)).ToArray()));
            }).ToArray(), application.CreatedByUserId);
    }

    public async Task<DesignerObjectResponse> SaveDraft(SaveDesignerDraftRequest request, CancellationToken token)
    {
        if (!string.IsNullOrWhiteSpace(request.PageNodeId) && request.PageId is null) throw new ArgumentException("A form block binding requires its page ID.");
        if (request.Fields.Count > 100) throw new ArgumentException("A form can have at most 100 fields.");
        if (request.Fields.GroupBy(x => x.Key.Trim(), StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1)) throw new ArgumentException("Field keys must be unique.");
        if (request.Fields.Any(x => string.IsNullOrWhiteSpace(x.Key) || string.IsNullOrWhiteSpace(x.Label) || !Enum.IsDefined(x.Type))) throw new ArgumentException("Every field needs a valid key, label, and type.");
        if (string.IsNullOrWhiteSpace(request.ObjectKey) || string.IsNullOrWhiteSpace(request.ObjectName) || string.IsNullOrWhiteSpace(request.FormKey) || string.IsNullOrWhiteSpace(request.FormName)) throw new ArgumentException("Object and form names and keys are required.");
        if (request.LayoutJson is not null) { using var _ = JsonDocument.Parse(request.LayoutJson); }
        foreach (var field in request.Fields.Where(x => x.ConfigurationJson is not null)) { using var _ = JsonDocument.Parse(field.ConfigurationJson!); }

        // All changes are persisted by one SaveChanges call. EF owns its atomic
        // transaction so Npgsql's configured execution strategy can retry it.
        var application = await db.Applications.SingleOrDefaultAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token) ?? throw new KeyNotFoundException("Application not found for this tenant.");
        if (application.Status != DefinitionStatus.Draft) throw new InvalidOperationException("Published applications cannot be edited in the draft designer.");
        await ValidateFieldConfiguration(request, token);
        BuilderObject obj;
        if (request.ObjectId is Guid objectId)
        {
            obj = await db.Objects.SingleOrDefaultAsync(x => x.Id == objectId && x.TenantId == request.TenantId && x.ApplicationId == application.Id, token) ?? throw new KeyNotFoundException("Object not found for this application.");
            if (obj.Status != DefinitionStatus.Draft) throw new InvalidOperationException("Published objects cannot be edited in the draft designer.");
        }
        else
        {
            obj = new BuilderObject { TenantId = request.TenantId, ApplicationId = application.Id, Key = request.ObjectKey.Trim(), Name = request.ObjectName.Trim() };
            db.Objects.Add(obj);
        }
        obj.Key = request.ObjectKey.Trim(); obj.Name = request.ObjectName.Trim(); obj.UpdatedAt = DateTimeOffset.UtcNow;
        var form = request.FormId is Guid formId
            ? await db.Forms.SingleOrDefaultAsync(x => x.Id == formId && x.TenantId == request.TenantId && x.ObjectId == obj.Id, token) ?? throw new KeyNotFoundException("Form not found for this object.")
            : await db.Forms.SingleOrDefaultAsync(x => x.TenantId == request.TenantId && x.ObjectId == obj.Id && x.Key == request.FormKey, token);
        if (form is null) { form = new BuilderForm { TenantId = request.TenantId, ObjectId = obj.Id, Key = request.FormKey.Trim(), Name = request.FormName.Trim() }; db.Forms.Add(form); }
        if (form.Status != DefinitionStatus.Draft) throw new InvalidOperationException("Published forms cannot be edited in the draft designer.");
        form.Key = request.FormKey.Trim(); form.Name = request.FormName.Trim(); form.LayoutJson = request.LayoutJson; form.UpdatedAt = DateTimeOffset.UtcNow;

        var existing = await db.Fields.Where(x => x.TenantId == request.TenantId && x.ObjectId == obj.Id).ToListAsync(token);
        var incomingIds = request.Fields.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();
        if (incomingIds.Any(id => existing.All(x => x.Id != id))) throw new ArgumentException("A field does not belong to this object.");
        db.Fields.RemoveRange(existing.Where(x => !incomingIds.Contains(x.Id)));
        foreach (var item in request.Fields)
        {
            var field = item.Id is Guid id ? existing.Single(x => x.Id == id) : new BuilderField { TenantId = request.TenantId, ObjectId = obj.Id, Key = item.Key.Trim(), Label = item.Label.Trim() };
            field.Key = item.Key.Trim(); field.Label = item.Label.Trim(); field.Type = item.Type; field.SortOrder = item.SortOrder;
            field.Required = item.Required; field.Searchable = item.Searchable;
            field.ReadOnly = item.ReadOnly || HasFormula(item.ConfigurationJson); field.ConfigurationJson = item.ConfigurationJson;
            field.UpdatedAt = DateTimeOffset.UtcNow;
            if (item.Id is null) db.Fields.Add(field);
        }
        if (request.ModuleId is Guid moduleId)
        {
            var module = await db.ApplicationModules.SingleOrDefaultAsync(x => x.Id == moduleId && x.ApplicationId == application.Id && x.TenantId == request.TenantId, token)
                ?? throw new KeyNotFoundException("Module not found for this application.");
            if (module.ObjectId is Guid existingObjectId && existingObjectId != obj.Id)
                throw new InvalidOperationException("This module already has a data object. Select another module or edit its existing object.");
            module.ObjectId = obj.Id; module.FormId = form.Id; module.UpdatedAt = DateTimeOffset.UtcNow; module.Version++;
            if (module.ScreenId is Guid screenId)
            {
                var screen = await db.ApplicationScreens.SingleAsync(x => x.Id == screenId && x.ApplicationId == application.Id && x.TenantId == request.TenantId, token);
                screen.ObjectId = obj.Id; screen.FormKey = form.Key;
            }
        }
        if (request.PageId is Guid pageId) {
            var page = await db.ApplicationScreens.SingleOrDefaultAsync(x => x.Id == pageId && x.ApplicationId == application.Id && x.TenantId == request.TenantId, token) ?? throw new KeyNotFoundException("Page not found for this application.");
            if (!string.IsNullOrWhiteSpace(request.PageNodeId)) {
                var definition = System.Text.Json.Nodes.JsonNode.Parse(page.DefinitionJson ?? "{}");
                static System.Text.Json.Nodes.JsonNode? GetProperty(System.Text.Json.Nodes.JsonObject value, string name) =>
                    value.FirstOrDefault(property => string.Equals(property.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
                System.Text.Json.Nodes.JsonObject? Find(System.Text.Json.Nodes.JsonNode? nodes) {
                    if (nodes is not System.Text.Json.Nodes.JsonArray array) return null;
                    foreach (var item in array.OfType<System.Text.Json.Nodes.JsonObject>()) {
                        if (string.Equals(GetProperty(item, "id")?.GetValue<string>(), request.PageNodeId, StringComparison.Ordinal)) return item;
                        var match = Find(GetProperty(item, "children")); if (match is not null) return match;
                    }
                    return null;
                }
                var node = definition is System.Text.Json.Nodes.JsonObject root ? Find(GetProperty(root, "children")) : null;
                if (node is null || !string.Equals(GetProperty(node, "type")?.GetValue<string>(), "form", StringComparison.OrdinalIgnoreCase))
                    throw new FormBlockNotFoundException("The selected form block is no longer on this page. Refresh the page designer and select the form block again.");
                static void SetProperty(System.Text.Json.Nodes.JsonObject value, string name, string propertyValue)
                {
                    var existingName = value.Select(property => property.Key).FirstOrDefault(key => string.Equals(key, name, StringComparison.OrdinalIgnoreCase)) ?? name;
                    value[existingName] = propertyValue;
                }
                SetProperty(node, "objectId", obj.Id.ToString()); SetProperty(node, "formId", form.Id.ToString());
                page.DefinitionJson = definition!.ToJsonString();
            } else { page.ObjectId = obj.Id; page.FormId = form.Id; page.FormKey = form.Key; }
            page.Version++; page.UpdatedAt = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(token);
        return new DesignerObjectResponse(obj.Id, obj.Key, obj.Name, obj.Status.ToString(), form.Id, form.Key, form.Name, form.LayoutJson,
            await db.Fields.AsNoTracking().Where(x => x.TenantId == request.TenantId && x.ObjectId == obj.Id).OrderBy(x => x.SortOrder)
                .Select(x => new DesignerFieldResponse(x.Id, x.Key, x.Label, x.Type, x.SortOrder, x.Required, x.Searchable, x.ReadOnly, x.ConfigurationJson)).ToArrayAsync(token),
            await db.Forms.AsNoTracking().Where(x => x.TenantId == request.TenantId && x.ObjectId == obj.Id)
                .OrderBy(x => x.CreatedAt).Select(x => new DesignerFormResponse(x.Id, x.Key, x.Name, x.LayoutJson)).ToArrayAsync(token));
    }

    private static bool HasFormula(string? configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson)) return false;
        using var config = JsonDocument.Parse(configurationJson);
        return config.RootElement.ValueKind == JsonValueKind.Object && config.RootElement.TryGetProperty("formula", out _);
    }

    private async Task ValidateFieldConfiguration(SaveDesignerDraftRequest request, CancellationToken token)
    {
        foreach (var field in request.Fields)
        {
            if (field.Type != BuilderFieldType.Lookup && !HasFormula(field.ConfigurationJson)) continue;
            if (string.IsNullOrWhiteSpace(field.ConfigurationJson)) throw new ArgumentException($"Field '{field.Key}' needs configuration.");
            using var config = JsonDocument.Parse(field.ConfigurationJson);
            if (config.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException($"Field '{field.Key}' configuration must be an object.");
            if (field.Type == BuilderFieldType.Lookup)
            {
                if (!config.RootElement.TryGetProperty("lookup", out var lookup) || lookup.ValueKind != JsonValueKind.Object ||
                    !lookup.TryGetProperty("targetObjectId", out var target) || !Guid.TryParse(target.GetString(), out var targetId) ||
                    !lookup.TryGetProperty("displayField", out var display) || string.IsNullOrWhiteSpace(display.GetString()))
                    throw new ArgumentException($"Lookup field '{field.Key}' needs a target table and display field.");
                var targetObject = await db.Objects.AsNoTracking().AnyAsync(x => x.Id == targetId && x.TenantId == request.TenantId && x.ApplicationId == request.ApplicationId, token);
                var displayExists = await db.Fields.AsNoTracking().AnyAsync(x => x.ObjectId == targetId && x.TenantId == request.TenantId && x.Key == display.GetString(), token);
                if (!targetObject || !displayExists) throw new ArgumentException($"Lookup field '{field.Key}' must target a field in this application.");
            }
            if (config.RootElement.TryGetProperty("formula", out var formula))
            {
                if (field.Type != BuilderFieldType.Decimal || formula.ValueKind != JsonValueKind.Object ||
                    !formula.TryGetProperty("left", out var left) || !formula.TryGetProperty("right", out var right) ||
                    !formula.TryGetProperty("operator", out var operation) || operation.GetString() is not ("+" or "-" or "*" or "/"))
                    throw new ArgumentException($"Calculated field '{field.Key}' needs a decimal type and two numeric fields.");
                foreach (var key in new[] { left.GetString(), right.GetString() })
                {
                    var source = request.Fields.SingleOrDefault(x => x.Key == key);
                    if (source is null || source.Key == field.Key || source.Type is not (BuilderFieldType.Decimal or BuilderFieldType.Integer) || HasFormula(source.ConfigurationJson))
                        throw new ArgumentException($"Calculated field '{field.Key}' may only use other base numeric fields.");
                }
            }
        }
    }
}
