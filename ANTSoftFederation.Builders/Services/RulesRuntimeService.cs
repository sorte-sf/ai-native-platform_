using SorteFederation.Builders.Contracts;
using System.Globalization;
using System.Text.Json;

namespace SorteFederation.Builders.Services;

public sealed class RulesRuntimeService(AuditService audit, IHttpContextAccessor accessor)
{
    public static void ValidateDefinition(string rulesJson)
    {
        if (string.IsNullOrWhiteSpace(rulesJson) || rulesJson.Length > 1_000_000)
            throw new ArgumentException("A rule definition is required and must be no larger than 1 MB.");
        try
        {
            using var document = JsonDocument.Parse(rulesJson);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("A rule definition must be a JSON object.");
            if (root.TryGetProperty("trigger", out var trigger) &&
                (trigger.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(trigger.GetString()) || trigger.GetString()!.Length > 160))
                throw new ArgumentException("Rule trigger must be a non-empty event name no longer than 160 characters.");
            if (root.TryGetProperty("actions", out var actionDefinitions) &&
                actionDefinitions.ValueKind == JsonValueKind.Array &&
                actionDefinitions.EnumerateArray().Any(action => action.ValueKind == JsonValueKind.String && action.GetString()!.StartsWith("connector.", StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Connector actions must be invoked through the authenticated connector execution endpoint.");
            var budget = 0;
            ValidateNode(root, "rule", 0, ref budget, isRoot: true);
            if (root.TryGetProperty("actions", out var actions))
            {
                if (actions.ValueKind != JsonValueKind.Array || actions.GetArrayLength() > 20)
                    throw new ArgumentException("Rule actions must be an array containing at most 20 actions.");
                foreach (var action in actions.EnumerateArray())
                    if (action.ValueKind != JsonValueKind.String || !IsApprovedAction(action.GetString()!))
                        throw new ArgumentException("Rule actions must use an approved event, notification, workflow, or connector action type.");
            }
        }
        catch (JsonException ex) { throw new ArgumentException("Rule definition must be valid JSON.", ex); }
    }

    private static void ValidateNode(JsonElement node, string path, int depth, ref int budget, bool isRoot = false)
    {
        if (depth > 16 || ++budget > 500)
            throw new ArgumentException("Rule conditions exceed the supported nesting or size limit.");
        if (node.ValueKind != JsonValueKind.Object)
            throw new ArgumentException($"{path} must be an object.");
        var allowedProperties = new HashSet<string>(StringComparer.Ordinal) { "all", "any", "field", "operator", "value" };
        if (isRoot) { allowedProperties.Add("trigger"); allowedProperties.Add("actions"); }
        if (node.EnumerateObject().Any(property => !allowedProperties.Contains(property.Name)))
            throw new ArgumentException($"{path} contains an unsupported property.");
        var hasAll = node.TryGetProperty("all", out var all);
        var hasAny = node.TryGetProperty("any", out var any);
        if (hasAll || hasAny)
        {
            if (hasAll && hasAny) throw new ArgumentException($"{path} cannot contain both all and any groups.");
            var group = hasAll ? all : any;
            if (group.ValueKind != JsonValueKind.Array || group.GetArrayLength() is < 1 or > 100)
                throw new ArgumentException($"{path} condition group must contain 1–100 conditions.");
            var index = 0;
            foreach (var child in group.EnumerateArray()) ValidateNode(child, $"{path}.{(hasAll ? "all" : "any")}[{index++}]", depth + 1, ref budget);
            return;
        }
        if (!node.TryGetProperty("field", out var field) || field.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(field.GetString()) || field.GetString()!.Length > 200 ||
            !node.TryGetProperty("operator", out var op) || op.ValueKind != JsonValueKind.String)
            throw new ArgumentException($"{path} requires a field and operator.");
        var operatorName = op.GetString()!.ToLowerInvariant();
        if (operatorName is not ("eq" or "neq" or "contains" or "startswith" or "endswith" or "gt" or "gte" or "lt" or "lte" or "isnull" or "isnotnull"))
            throw new ArgumentException($"{path} uses unsupported operator '{operatorName}'.");
        if (operatorName is not ("isnull" or "isnotnull") && (!node.TryGetProperty("value", out var value) || value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Array || value.ToString().Length > 4000))
            throw new ArgumentException($"{path} requires a scalar value for operator '{operatorName}'.");
    }

    private static bool IsApprovedAction(string action) =>
        action.Length <= 160 && (action.StartsWith("event.", StringComparison.OrdinalIgnoreCase) ||
            action.StartsWith("notification.", StringComparison.OrdinalIgnoreCase) ||
            action.StartsWith("workflow.", StringComparison.OrdinalIgnoreCase) ||
            action.StartsWith("connector.", StringComparison.OrdinalIgnoreCase));

    public async Task<EvaluateRulesResponse> EvaluateAsync(EvaluateRulesRequest request, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.RulesJson) || string.IsNullOrWhiteSpace(request.DataJson))
            throw new ArgumentException("RulesJson and DataJson are required.");
        try
        {
            using var rules = JsonDocument.Parse(request.RulesJson);
            using var data = JsonDocument.Parse(request.DataJson);
            ValidateDefinition(request.RulesJson);
            if (data.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("Rule data must be a JSON object.");
            var errors = new List<string>();
            var matched = Evaluate(rules.RootElement, data.RootElement, errors, "rule");
            var actions = matched && rules.RootElement.TryGetProperty("actions", out var actionNode) && actionNode.ValueKind == JsonValueKind.Array
                ? actionNode.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!).Where(x => x.Length <= 160).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                : Array.Empty<string>();
            var result = new EvaluateRulesResponse(matched, errors, actions);
            await audit.Record(new CreateAuditEventRequest(request.TenantId, "rules.evaluated", "rules", null,
                JsonSerializer.Serialize(new { matched, errorCount = errors.Count })), user, accessor.HttpContext?.TraceIdentifier, token);
            return result;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"RulesJson or DataJson is invalid: {ex.Message}");
        }
    }

    private static bool Evaluate(JsonElement node, JsonElement data, List<string> errors, string path)
    {
        if (node.ValueKind != JsonValueKind.Object) { errors.Add($"{path} must be an object."); return false; }
        if (node.TryGetProperty("all", out var all))
            return all.EnumerateArray().Select((x, i) => Evaluate(x, data, errors, $"{path}.all[{i}]" )).All(x => x);
        if (node.TryGetProperty("any", out var any))
            return any.EnumerateArray().Select((x, i) => Evaluate(x, data, errors, $"{path}.any[{i}]" )).Any(x => x);
        if (!node.TryGetProperty("field", out var field) || !node.TryGetProperty("operator", out var op) || field.ValueKind != JsonValueKind.String || op.ValueKind != JsonValueKind.String)
        { errors.Add($"{path} requires field and operator."); return false; }
        if (!data.TryGetProperty(field.GetString()!, out var actual)) return false;
        var expected = node.TryGetProperty("value", out var value) ? value.ToString() : null;
        var current = actual.ToString();
        return op.GetString()!.ToLowerInvariant() switch
        {
            "eq" => string.Equals(current, expected, StringComparison.OrdinalIgnoreCase),
            "neq" => !string.Equals(current, expected, StringComparison.OrdinalIgnoreCase),
            "contains" => current.Contains(expected ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "startswith" => current.StartsWith(expected ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "endswith" => current.EndsWith(expected ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "gt" => Compare(current, expected) > 0,
            "gte" => Compare(current, expected) >= 0,
            "lt" => Compare(current, expected) < 0,
            "lte" => Compare(current, expected) <= 0,
            "isNull" => actual.ValueKind == JsonValueKind.Null,
            "isNotNull" => actual.ValueKind != JsonValueKind.Null,
            _ => InvalidOperator(op.GetString()!, errors, path)
        };
    }

    private static int Compare(string current, string? expected) =>
        decimal.TryParse(current, NumberStyles.Any, CultureInfo.InvariantCulture, out var a) && decimal.TryParse(expected, NumberStyles.Any, CultureInfo.InvariantCulture, out var b) ? a.CompareTo(b) : string.Compare(current, expected, StringComparison.OrdinalIgnoreCase);
    private static bool InvalidOperator(string op, List<string> errors, string path) { errors.Add($"{path} uses unsupported operator '{op}'."); return false; }
}
