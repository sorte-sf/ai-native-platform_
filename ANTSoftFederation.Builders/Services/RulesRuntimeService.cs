using SorteFederation.Builders.Contracts;
using System.Globalization;
using System.Text.Json;

namespace SorteFederation.Builders.Services;

public sealed class RulesRuntimeService(AuditService audit, IHttpContextAccessor accessor)
{
    public async Task<EvaluateRulesResponse> EvaluateAsync(EvaluateRulesRequest request, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.RulesJson) || string.IsNullOrWhiteSpace(request.DataJson))
            throw new ArgumentException("RulesJson and DataJson are required.");
        try
        {
            using var rules = JsonDocument.Parse(request.RulesJson);
            using var data = JsonDocument.Parse(request.DataJson);
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
            return all.ValueKind == JsonValueKind.Array && all.EnumerateArray().Select((x, i) => Evaluate(x, data, errors, $"{path}.all[{i}]" )).All(x => x);
        if (node.TryGetProperty("any", out var any))
            return any.ValueKind == JsonValueKind.Array && any.EnumerateArray().Select((x, i) => Evaluate(x, data, errors, $"{path}.any[{i}]" )).Any(x => x);
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
