namespace SorteFederation.Builders.Services;

public sealed record PageNode(string Id, string Type, string? Text = null, Guid? ObjectId = null, Guid? FormId = null, List<PageNode>? Children = null,
    string? ImageUrl = null, string? PageKey = null, List<string>? Columns = null, Dictionary<string, string>? Styling = null,
    Dictionary<string, List<double>>? ResponsiveColumns = null, string? Presentation = null,
    List<PageTableColumn>? TableColumns = null, string? TableJson = null, PageCrudActions? CrudActions = null,
    string? ChartKind = null, string? ValueField = null, string? TitleField = null, string? SecondaryValueField = null,
    int? GalleryColumns = null, int? PageSize = null, string? ImageField = null, string? TargetNodeId = null,
    string? ActionType = null, string? ActionValue = null, string? NavigationPlacement = null,
    string? NavigationLogoUrl = null, bool FloatingButton = false);
[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record PageCrudActions(bool Create = true, bool View = true, bool Edit = true, bool Delete = true);
public sealed record PageTableColumn(string Field, string Title, int? Width = null, string? Align = null, string? Format = null, bool Hidden = false);
public sealed record PageDefinition(int SchemaVersion, List<PageNode> Children);

public sealed class PageDefinitionService(BuilderDbContext db)
{
    public async Task<PageDefinition> Validate(Guid tenantId, Guid applicationId, string json, CancellationToken token, bool publishing = false)
    {
        if (json.Length > 500_000) throw new ArgumentException("Page definition exceeds 500 KB.");
        PageDefinition definition;
        try { definition = JsonSerializer.Deserialize<PageDefinition>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new ArgumentException("Page definition is required."); }
        catch (JsonException) { throw new ArgumentException("Invalid page definition JSON."); }
        if (definition.SchemaVersion != PageComponentCapabilities.SchemaVersion || definition.Children is null) throw new ArgumentException("Expected a version 1 page component tree.");
        var ids = new HashSet<string>();
        var nodeTypes = new Dictionary<string, string>();
        var paginationTargets = new List<string>();
        var count = 0;
        var objects = await db.Objects.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId).Select(x => x.Id).ToListAsync(token);
        objects.AddRange(db.Objects.Local.Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId &&
            db.Entry(x).State != EntityState.Deleted).Select(x => x.Id));
        objects = objects.Distinct().ToList();
        var forms = await db.Forms.AsNoTracking().Where(x => x.TenantId == tenantId && objects.Contains(x.ObjectId)).Select(x => new { x.Id, x.ObjectId }).ToListAsync(token);
        forms.AddRange(db.Forms.Local.Where(x => x.TenantId == tenantId && objects.Contains(x.ObjectId) &&
            db.Entry(x).State != EntityState.Deleted).Select(x => new { x.Id, x.ObjectId }));
        forms = forms.DistinctBy(x => x.Id).ToList();
        var pages = await db.ApplicationScreens.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId).Select(x => x.Key).ToListAsync(token);
        pages.AddRange(db.ApplicationScreens.Local.Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId &&
            db.Entry(x).State != EntityState.Deleted).Select(x => x.Key));
        pages = pages.Distinct(StringComparer.Ordinal).ToList();
        var fields = await db.Fields.AsNoTracking().Where(x => x.TenantId == tenantId && objects.Contains(x.ObjectId)).Select(x => new { x.ObjectId, x.Key }).ToListAsync(token);
        fields.AddRange(db.Fields.Local.Where(x => x.TenantId == tenantId && objects.Contains(x.ObjectId) &&
            db.Entry(x).State != EntityState.Deleted).Select(x => new { x.ObjectId, x.Key }));
        fields = fields.DistinctBy(x => (x.ObjectId, x.Key)).ToList();
        // Build once per definition, avoiding repeated linear scans for every node/column.
        var objectIds = objects.ToHashSet();

        var formBindings = forms.Select(form => (form.Id, form.ObjectId)).ToHashSet();

        var fieldBindings = fields.Select(field => (field.ObjectId, field.Key)).ToHashSet();

        var pageKeys = pages.ToHashSet(StringComparer.Ordinal);

        var allowedStyles = new HashSet<string>("display text-align width height min-width min-height max-width max-height position container-type left top aspect-ratio grid-column grid-template-columns padding-top padding-right padding-bottom padding-left margin-top margin-right margin-bottom margin-left gap font-family font-weight font-size line-height letter-spacing color text-transform text-decoration font-style background-color border-color border-style border-width border-radius opacity object-fit".Split(' '));

        async Task Check(PageNode node, int depth)
        {
            if (node is null || depth > 12 || ++count > 500) throw new ArgumentException("Page tree exceeds its size or depth limit.");
            if (string.IsNullOrWhiteSpace(node.Id) || node.Id.Length > 100 || !ids.Add(node.Id)) throw new ArgumentException("Every component needs a unique ID.");
            if (node.Type is null || !PageComponentCapabilities.TryGet(node.Type, out var supportsChildren)) throw new ArgumentException("Unsupported component type.");
            if (node.FloatingButton && node.Type != "button") throw new ArgumentException("Floating positioning is only supported for button components.");
            if (node.ActionType is not null && (node.Type != "button" || node.ActionType is not ("none" or "page" or "url" or "form")
                || node.ActionType == "form" && !node.FloatingButton))
                throw new ArgumentException("Invalid button action type.");
            if (node.ActionValue is { Length: > 2048 }) throw new ArgumentException("Button action value exceeds 2048 characters.");
            if (node.ActionType == "page")
            {
                var target = node.ActionValue ?? node.PageKey;
                if (target is { Length: > 0 } && (!System.Text.RegularExpressions.Regex.IsMatch(target, "^[a-z][a-z0-9-]{0,79}$") || !pageKeys.Contains(target)))
                    throw new ArgumentException("Button page action must target a page in this application.");
                if (publishing && string.IsNullOrWhiteSpace(target)) throw new ArgumentException("Button page action needs a target page.");
            }
            if (node.ActionType == "url")
            {
                if (publishing && !string.IsNullOrWhiteSpace(node.ActionValue) && !IsSafeHttpsUrl(node.ActionValue))
                    throw new ArgumentException("Button links must use a valid HTTPS URL without credentials.");
                if (publishing && !IsSafeHttpsUrl(node.ActionValue)) throw new ArgumentException("Button URL action needs a valid HTTPS URL.");
            }
            if (node.ActionType == "form")
            {
                var bindingIsComplete = node.FormId is Guid formId && node.ObjectId is Guid formObject && formBindings.Contains((formId, formObject));
                if ((node.FormId is not null || node.ObjectId is not null) && !bindingIsComplete)
                    throw new ArgumentException("Floating button form must belong to this application and object.");
                if (publishing && !bindingIsComplete)
                    throw new ArgumentException("Floating button needs a form and data object before publishing.");
            }
            nodeTypes[node.Id] = node.Type;
            if (node.TargetNodeId is not null)
            {
                if (node.Type != "pagination" || node.TargetNodeId.Length > 100) throw new ArgumentException("Invalid pagination target.");
                paginationTargets.Add(node.TargetNodeId);
            }
            if (publishing && node.Type == "pagination" && string.IsNullOrWhiteSpace(node.TargetNodeId)) throw new ArgumentException("Pagination requires a target gallery.");
            if (publishing && node.Type == "gallery" && string.IsNullOrWhiteSpace(node.ImageField)) throw new ArgumentException("Gallery requires an image field.");
            if (node.Presentation is not null && (node.Type != "container" || node.Presentation is not ("collapse" or "tabs" or "stepper"))) throw new ArgumentException("Invalid container presentation.");
            if (node.NavigationPlacement is not null && (node.Type != "container" || node.Text != "Side navigation" || node.NavigationPlacement is not ("left" or "right" or "top"))) throw new ArgumentException("Invalid navigation placement.");
            if (node.NavigationLogoUrl is { } logo && (logo.Length > 2048 || node.Type != "container" || node.Text != "Side navigation" ||
                logo.Length > 0 && !(logo.StartsWith("/", StringComparison.Ordinal) && !logo.StartsWith("//", StringComparison.Ordinal) && !logo.Contains('\\')
                    || Uri.TryCreate(logo, UriKind.Absolute, out var logoUri) && logoUri.Scheme == "https" && !string.IsNullOrWhiteSpace(logoUri.Host) && string.IsNullOrEmpty(logoUri.UserInfo))))
                throw new ArgumentException("Navigation logos must use HTTPS or a local asset path.");
            if (!supportsChildren && node.Children?.Count > 0) throw new ArgumentException("Only containers can contain children.");
            PageComponentCapabilities.ValidateProperties(node);
            if (node.CrudActions is not null && node.Type != "crud") throw new ArgumentException("CRUD actions require a CRUD component.");
            if (node.ResponsiveColumns is not null && (node.Type != "container" || node.ResponsiveColumns.Any(pair =>
                pair.Key is not ("desktop" or "tablet" or "mobile") || pair.Value is null || pair.Value.Count is < 1 or > 12 ||
                pair.Value.Any(value => !double.IsFinite(value) || value <= 0 || value > 24))))
                throw new ArgumentException("Responsive columns require a container and valid desktop, tablet or mobile proportions.");
            var binding = PageComponentCapabilities.BindingFor(node.Type);
            if (node.ChartKind is not null && (node.Type != "chart" || node.ChartKind is not ("bar" or "line" or "area" or "stacked-column" or "time-area" or "bar-line" or "pie" or "doughnut" or "funnel"))) throw new ArgumentException("Invalid chart type.");
            if (node.ValueField is { Length: > 0 } valueField && (node.Type != "chart" || node.ObjectId is not Guid valueObject || !fieldBindings.Contains((valueObject, valueField)))) throw new ArgumentException("Chart value field must belong to its data source.");
            if (node.SecondaryValueField is { Length: > 0 } secondaryField && (node.Type != "chart" || node.ChartKind is not ("bar-line" or "stacked-column") || node.ObjectId is not Guid secondaryObject || !fieldBindings.Contains((secondaryObject, secondaryField)))) throw new ArgumentException("Secondary chart value field must belong to its data source.");
            if (node.TitleField is { Length: > 0 } titleField && (node.Type is not ("kanban" or "gallery") || titleField.Length > 100 || node.ObjectId is Guid titleObject && !fieldBindings.Contains((titleObject, titleField)))) throw new ArgumentException("Card title field must belong to its data source.");
            if (node.GalleryColumns is not null && (node.Type != "gallery" || node.GalleryColumns is < 1 or > 6)) throw new ArgumentException("Gallery columns must be between 1 and 6.");
            if (node.PageSize is not null && (node.Type is not ("gallery" or "data-grid") || node.PageSize is < 1 or > 100)) throw new ArgumentException("Invalid page size.");
            if (node.ImageField is not null && (node.Type != "gallery" || node.ImageField.Length > 100 || node.ObjectId is Guid imageObject && !fieldBindings.Contains((imageObject, node.ImageField)))) throw new ArgumentException("Invalid gallery image field.");
            if (node.Type == "kanban" && publishing && (node.Columns?.Count != 1 || string.IsNullOrWhiteSpace(node.TitleField))) throw new ArgumentException("Kanban needs a stage field and card title field.");
            if (node.ObjectId is Guid objectId && !objectIds.Contains(objectId)) throw new ArgumentException("Data source belongs to another application or tenant.");
            if (binding == "object-or-json" && publishing && node.ObjectId is null && string.IsNullOrWhiteSpace(node.TableJson)) throw new ArgumentException("Data table needs a data source before publishing.");
            if (node.TableJson is not null)
            {
                if (node.Type is not ("table" or "gallery" or "data-grid") || node.ObjectId is not null || node.TableJson.Length > 100_000) throw new ArgumentException("Invalid JSON table source.");

                try
                {
                    using var data = JsonDocument.Parse(node.TableJson);
                    if (data.RootElement.ValueKind != JsonValueKind.Array || data.RootElement.GetArrayLength() > 200 || data.RootElement.EnumerateArray().Any(row => row.ValueKind != JsonValueKind.Object))
                        throw new ArgumentException("Table JSON must contain at most 200 objects.");
                }
                catch (JsonException)
                {
                    throw new ArgumentException("Invalid table JSON.");
                }
            }
            if (node.TableColumns is not null)
            {
                if (node.Type is not ("table" or "crud" or "data-grid") || node.TableColumns.Count > 50 || node.TableColumns.Any(column => column is null || column.Field is null || column.Title is null || column.Title.Length > 200 || column.Field.Length > 100 || column.Width is < 40 or > 1200 || column.Align is not (null or "left" or "center" or "right") || column.Format is not (null or "text" or "number" or "date"))) throw new ArgumentException("Invalid table column settings.");

                if (publishing && node.TableColumns.Any(column => string.IsNullOrWhiteSpace(column.Field))) throw new ArgumentException("Every table column needs a field mapping.");

                if (node.ObjectId is Guid source && node.TableColumns.Any(column => !string.IsNullOrWhiteSpace(column.Field) && !fieldBindings.Contains((source, column.Field)))) throw new ArgumentException("Table column field does not belong to its data source.");
            }
            if (binding == "object" && publishing && node.ObjectId is null) throw new ArgumentException("Dashboard widget needs a data source before publishing.");

            if (node.Type == "chart" && publishing && node.Columns?.Count != 1) throw new ArgumentException("Chart needs exactly one grouping field before publishing.");

            if (node.PageKey is not null && !pageKeys.Contains(node.PageKey)) throw new ArgumentException("Navigation target is not a page in this application.");

            if (node.ImageUrl is { Length: > 0 } image && !(image.StartsWith("/", StringComparison.Ordinal) && !image.StartsWith("//") && !image.Contains('\\')
                || Uri.TryCreate(image, UriKind.Absolute, out var uri) && uri.Scheme == "https")) throw new ArgumentException("Images must use HTTPS or a local asset path.");

            if (node.Styling is not null && node.Styling.Any(pair => !allowedStyles.Contains(pair.Key) || pair.Value is null || pair.Value.Length > 100
                || !System.Text.RegularExpressions.Regex.IsMatch(pair.Key, "^[a-z-]+$") || !System.Text.RegularExpressions.Regex.IsMatch(pair.Value, "^[a-zA-Z0-9#.,% /_-]+$")
                || System.Text.RegularExpressions.Regex.IsMatch(pair.Value, "url|expression|javascript|important", System.Text.RegularExpressions.RegexOptions.IgnoreCase))) throw new ArgumentException("Invalid element styling.");

            if (node.Columns?.Count > 50) throw new ArgumentException("A table can show at most 50 columns.");

            if (node.Columns is not null && node.ObjectId is Guid tableObject && node.Columns.Any(key => !fieldBindings.Contains((tableObject, key)))) throw new ArgumentException("Table columns must reference fields in its data source.");

            if (binding == "form")
            {
                if ((publishing || node.FormId is not null || node.ObjectId is not null) && !(node.FormId is Guid formId && node.ObjectId is Guid formObject && formBindings.Contains((formId, formObject))))
                    throw new ArgumentException("Form component must reference a form and object belonging to this application.");
            }

            foreach (var child in node.Children ?? []) await Check(child, depth + 1);
        }
        foreach (var child in definition.Children) await Check(child, 1);
        if (paginationTargets.Any(target => !nodeTypes.TryGetValue(target, out var type) || type != "gallery")) throw new ArgumentException("Pagination target must be a gallery on this page.");
        return definition;
    }

    private static bool IsSafeHttpsUrl(string? value) => value is { Length: <= 2048 } &&
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps &&
        !string.IsNullOrWhiteSpace(uri.Host) && string.IsNullOrEmpty(uri.UserInfo);
}
