using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SorteFederation.Builders.Contracts;

namespace SorteFederation.Builders.Services;

public static partial class TemplateManifestService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [GeneratedRegex("^[a-z][a-z0-9-]{0,79}$")]
    private static partial Regex KeyPattern();

    [GeneratedRegex("<\\s*(script|iframe|object|embed|style)\\b|\\bon[a-z]+\\s*=|javascript\\s*:", RegexOptions.IgnoreCase)]
    private static partial Regex UnsafeMarkupPattern();

    public static TemplateManifest Parse(string json)
    {
        if (json.Length > 250_000) throw new ArgumentException("Template manifest exceeds 250 KB.");
        TemplateManifest manifest;
        try { manifest = JsonSerializer.Deserialize<TemplateManifest>(json, JsonOptions)
            ?? throw new ArgumentException("Template manifest is required."); }
        catch (JsonException ex) { throw new ArgumentException("Template manifest must be valid JSON.", ex); }
        if (manifest.Version < 1 || manifest.Objects is null || manifest.Screens is null ||
            manifest.Objects.Count is < 1 or > 20 || manifest.Screens.Count > 40 || manifest.Workflows?.Count > 20 || manifest.Relationships?.Count > 100)
            throw new ArgumentException("Template needs 1–20 objects and at most 40 screens.");
        Unique(manifest.Objects.Select(x => x.Key), "object");
        Unique(manifest.Screens.Select(x => x.Key), "screen");
        if (manifest.Relationships is not null)
        {
            Unique(manifest.Relationships.Select(x => x.Key), "relationship");
            foreach (var relationship in manifest.Relationships)
            {
                if (string.IsNullOrWhiteSpace(relationship.Name) || relationship.Name.Length > 120 ||
                    manifest.Objects.All(x => !x.Key.Equals(relationship.SourceObjectKey, StringComparison.OrdinalIgnoreCase)) ||
                    manifest.Objects.All(x => !x.Key.Equals(relationship.TargetObjectKey, StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException($"Relationship '{relationship.Key}' refers to an unknown object or has an invalid name.");
                _ = RelationshipCardinalityRules.Normalize(relationship.Cardinality);
            }
        }
        if (manifest.Navigation is not null)
        {
            var navigationKeys = new List<string>();
            void CheckNavigation(IEnumerable<TemplateNavigationItem> items, int depth)
            {
                if (depth > 2) throw new ArgumentException("Template navigation supports one sub-menu level.");
                foreach (var item in items)
                {
                    navigationKeys.Add(item.Key);
                    if (string.IsNullOrWhiteSpace(item.Label) || item.Label.Length > 120 ||
                        (item.Icon is not null && item.Icon.Length > 80) ||
                        (item.ScreenKey is not null && manifest.Screens.All(screen => screen.Key != item.ScreenKey)))
                        throw new ArgumentException($"Navigation item '{item.Key}' is invalid or references an unknown page.");
                    if (item.Children is { Count: > 20 }) throw new ArgumentException("A menu can contain up to 20 sub-menu items.");
                    if (item.Children is not null) CheckNavigation(item.Children, depth + 1);
                }
            }
            if (manifest.Navigation.Count > 50) throw new ArgumentException("A template can contain up to 50 top-level navigation items.");
            CheckNavigation(manifest.Navigation, 1);
            Unique(navigationKeys, "navigation item");
        }
        foreach (var obj in manifest.Objects)
        {
            if (string.IsNullOrWhiteSpace(obj.Name) || obj.Name.Length > 120 || obj.Fields is null || obj.Forms is null ||
                obj.Fields.Count is < 1 or > 100 || obj.Forms.Count > 10)
                throw new ArgumentException($"Object '{obj.Key}' is incomplete or too large.");
            Unique(obj.Fields.Select(x => x.Key), "field");
            Unique(obj.Forms.Select(x => x.Key), "form");
            foreach (var field in obj.Fields)
            {
                if (!Enum.IsDefined(field.Type) || string.IsNullOrWhiteSpace(field.Label) || field.Label.Length > 120)
                    throw new ArgumentException($"Field '{field.Key}' is invalid.");
                if (field.ConfigurationJson is not null) RequireObject(field.ConfigurationJson);
                if (field.Type == BuilderFieldType.Lookup)
                {
                    if (field.ConfigurationJson is null) throw new ArgumentException($"Lookup field '{field.Key}' needs a target object and display field.");
                    using var configuration = JsonDocument.Parse(field.ConfigurationJson);
                    if (!configuration.RootElement.TryGetProperty("lookup", out var lookup) || lookup.ValueKind != JsonValueKind.Object ||
                        !lookup.TryGetProperty("targetObjectKey", out var targetKey) || targetKey.ValueKind != JsonValueKind.String ||
                        !lookup.TryGetProperty("displayField", out var displayField) || displayField.ValueKind != JsonValueKind.String)
                        throw new ArgumentException($"Lookup field '{field.Key}' must use targetObjectKey and displayField.");
                    var target = manifest.Objects.SingleOrDefault(candidate => candidate.Key == targetKey.GetString());
                    if (target is null || !target.Fields.Any(candidate => candidate.Key == displayField.GetString()))
                        throw new ArgumentException($"Lookup field '{field.Key}' refers to an unknown object or display field.");
                }
            }
            foreach (var form in obj.Forms)
            {
                if (string.IsNullOrWhiteSpace(form.Name) || form.Name.Length > 120)
                    throw new ArgumentException($"Form '{form.Key}' is invalid.");
                using var document = JsonDocument.Parse(form.FormConfigJson);
                if (document.RootElement.ValueKind != JsonValueKind.Object ||
                    !document.RootElement.TryGetProperty("fields", out var fields) || fields.ValueKind != JsonValueKind.Array)
                    throw new ArgumentException($"Form '{form.Key}' must contain ng-forge fields.");
                var declared = obj.Fields.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var item in fields.EnumerateArray())
                    if (!item.TryGetProperty("key", out var key) ||
                        (key.GetString() != "submit" && !declared.Contains(key.GetString() ?? "")))
                        throw new ArgumentException($"Form '{form.Key}' refers to an unknown field.");
            }
        }
        foreach (var screen in manifest.Screens)
        {
            var obj = manifest.Objects.FirstOrDefault(x => x.Key == screen.ObjectKey)
                ?? throw new ArgumentException($"Screen '{screen.Key}' refers to an unknown object.");
            if (string.IsNullOrWhiteSpace(screen.Title) || screen.Title.Length > 120 ||
                screen.HtmlContent.Length > 32_000 ||
                UnsafeMarkupPattern().IsMatch(screen.HtmlContent) ||
                (screen.FormKey is not null && obj.Forms.All(x => x.Key != screen.FormKey)))
                throw new ArgumentException($"Screen '{screen.Key}' is invalid.");
            if (screen.DefinitionJson is not null) ClonePage(screen.DefinitionJson, manifest, new Dictionary<string, Guid>(), new Dictionary<string, Guid>());
        }
        if (manifest.Workflows is not null)
        {
            Unique(manifest.Workflows.Select(workflow => workflow.Key), "workflow");
            foreach (var workflow in manifest.Workflows)
            {
                if (string.IsNullOrWhiteSpace(workflow.Name) || workflow.Name.Length > 120 || string.IsNullOrWhiteSpace(workflow.DefinitionJson))
                    throw new ArgumentException($"Workflow '{workflow.Key}' is invalid.");
                using var definition = JsonDocument.Parse(workflow.DefinitionJson);
                if (definition.RootElement.ValueKind != JsonValueKind.Object ||
                    !definition.RootElement.TryGetProperty("tasks", out var tasks) || tasks.ValueKind != JsonValueKind.Array)
                    throw new ArgumentException($"Workflow '{workflow.Key}' must define a task sequence.");
            }
        }
        return manifest;
    }

    /// <summary>
    /// Normalizes navigation keys from older Designer Studio releases. Menu labels and
    /// page references remain unchanged; only invalid or duplicate internal keys change.
    /// </summary>
    public static string NormalizeImportedNavigationKeys(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object ||
            !document.RootElement.TryGetProperty("navigation", out var navigation) ||
            navigation.ValueKind != JsonValueKind.Array)
            return json;

        var root = System.Text.Json.Nodes.JsonNode.Parse(json) as System.Text.Json.Nodes.JsonObject
            ?? throw new ArgumentException("Template manifest must be a JSON object.");
        var navNodes = root["navigation"]?.AsArray();
        if (navNodes is null) return json;

        var usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Normalize(System.Text.Json.Nodes.JsonArray items)
        {
            foreach (var item in items)
            {
                if (item is not System.Text.Json.Nodes.JsonObject menu) continue;
                var label = menu["label"]?.GetValue<string>() ?? "menu";
                var requestedKey = menu["key"]?.GetValue<string>();
                var baseKey = ToNavigationSlug(requestedKey, label);
                var key = baseKey;
                var suffix = 2;
                while (!usedKeys.Add(key))
                {
                    var suffixText = $"-{suffix++}";
                    key = baseKey[..Math.Min(baseKey.Length, 80 - suffixText.Length)] + suffixText;
                }
                menu["key"] = key;
                if (menu["children"] is System.Text.Json.Nodes.JsonArray children) Normalize(children);
            }
        }

        Normalize(navNodes);
        return root.ToJsonString();
    }

    private static string ToNavigationSlug(string? requestedKey, string label)
    {
        var source = string.IsNullOrWhiteSpace(requestedKey) ? label : requestedKey;
        var slug = Regex.Replace(source.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (slug.Length == 0 || !char.IsAsciiLetter(slug[0])) slug = "menu-" + slug;
        return slug[..Math.Min(slug.Length, 80)].TrimEnd('-') is { Length: > 0 } valid ? valid : "menu";
    }

    public static string? ResolveFieldConfiguration(string? json, IReadOnlyDictionary<string, Guid> objectIds)
    {
        if (json is null) return null;
        var root = System.Text.Json.Nodes.JsonNode.Parse(json)?.AsObject()
            ?? throw new ArgumentException("Field configuration must be a JSON object.");
        if (root["lookup"] is System.Text.Json.Nodes.JsonObject lookup && lookup["targetObjectKey"]?.GetValue<string>() is { } key)
        {
            if (!objectIds.TryGetValue(key, out var objectId)) throw new ArgumentException("Lookup refers to an unknown template object.");
            lookup.Remove("targetObjectKey");
            lookup["targetObjectId"] = objectId.ToString();
        }
        return root.ToJsonString();
    }

    public static string ClonePage(string json, TemplateManifest manifest, IReadOnlyDictionary<string, Guid> objects, IReadOnlyDictionary<string, Guid> forms)
    {
        var root = System.Text.Json.Nodes.JsonNode.Parse(json)?.AsObject() ?? throw new ArgumentException("Invalid template page.");
        if (root["schemaVersion"]?.GetValue<int>() != 1 || root["children"] is not System.Text.Json.Nodes.JsonArray children) throw new ArgumentException("Invalid template page schema.");
        var count = 0;
        var ids = new Dictionary<string, string>();
        var references = new List<(System.Text.Json.Nodes.JsonObject Node, string Target)>();
        void Visit(System.Text.Json.Nodes.JsonNode? value, int depth) {
            if (++count > 500 || depth > 12 || value is not System.Text.Json.Nodes.JsonObject node) throw new ArgumentException("Invalid template tree.");
            var type = node["type"]?.GetValue<string>() ?? "";
            if (!PageComponentCapabilities.TryGet(type, out var supportsChildren)) throw new ArgumentException("Unsupported template component.");
            var oldId = node["id"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(oldId) || oldId.Length > 100 || ids.ContainsKey(oldId)) throw new ArgumentException("Template component IDs must be unique.");
            var newId = Guid.NewGuid().ToString(); ids.Add(oldId, newId); node["id"] = newId;
            if (node["targetNodeId"] is not null) references.Add((node, node["targetNodeId"]!.GetValue<string>()));
            if (node.ContainsKey("objectId") || node.ContainsKey("formId")) throw new ArgumentException("Template bindings must use keys, not database IDs.");
            var objectKey = node["objectKey"]?.GetValue<string>();
            var formKey = node["formKey"]?.GetValue<string>();
            if (objectKey is not null) {
                var obj = manifest.Objects.SingleOrDefault(obj => obj.Key == objectKey) ?? throw new ArgumentException("Unknown template object key.");
                if (node["columns"] is System.Text.Json.Nodes.JsonArray columns && columns.Any(column => !obj.Fields.Any(field => field.Key == column?.GetValue<string>()))) throw new ArgumentException("Unknown template field binding.");
                if (formKey is not null && !obj.Forms.Any(form => form.Key == formKey)) throw new ArgumentException("Unknown template form key.");
                node.Remove("objectKey"); node.Remove("formKey");
                if (objects.TryGetValue(objectKey, out var id)) node["objectId"] = id.ToString();
                if (formKey is not null && forms.TryGetValue($"{objectKey}/{formKey}", out var formId)) node["formId"] = formId.ToString();
            } else if (formKey is not null) throw new ArgumentException("Template form requires an object key.");
            if (node["children"] is System.Text.Json.Nodes.JsonArray nested) {
                if (!supportsChildren && nested.Count > 0) throw new ArgumentException("Only containers can contain children.");
                foreach (var child in nested) Visit(child, depth + 1);
            }
        }
        foreach (var node in children) Visit(node, 1);
        foreach (var (node, target) in references) node["targetNodeId"] = ids.TryGetValue(target, out var id) ? id : throw new ArgumentException("Unknown template pagination target.");
        ApplyResponsiveGridDefaults(children);
        return root.ToJsonString();
    }

    private static void ApplyResponsiveGridDefaults(System.Text.Json.Nodes.JsonArray nodes)
    {
        static bool IsWidget(System.Text.Json.Nodes.JsonNode? value)
        {
            if (value is not System.Text.Json.Nodes.JsonObject node) return false;
            var type = node["type"]?.GetValue<string>();
            return type is "kpi" or "chart" or "table" or "data-grid" or "kanban" or "gallery" or "calendar" or "timeline" ||
                type == "container" && node["children"] is System.Text.Json.Nodes.JsonArray { Count: > 0 } children && children.All(IsWidget);
        }

        foreach (var item in nodes)
        {
            if (item is not System.Text.Json.Nodes.JsonObject node) continue;
            if (node["type"]?.GetValue<string>() == "container" && node["children"] is System.Text.Json.Nodes.JsonArray children)
            {
                if (children.Count >= 2 && children.All(IsWidget) && node["responsiveColumns"] is null)
                    node["responsiveColumns"] = System.Text.Json.Nodes.JsonNode.Parse("""
                        { "desktop": [8, 8, 8], "tablet": [12, 12], "mobile": [24] }
                        """);
                ApplyResponsiveGridDefaults(children);
            }
        }
    }

    private static void RequireObject(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Field configuration must be a JSON object.");
    }

    private static void Unique(IEnumerable<string> keys, string kind)
    {
        var values = keys.ToArray();
        if (values.Any(x => x is null || !KeyPattern().IsMatch(x)) ||
            values.Distinct(StringComparer.OrdinalIgnoreCase).Count() != values.Length)
            throw new ArgumentException($"Template {kind} keys must be unique lowercase slugs.");
    }
}
