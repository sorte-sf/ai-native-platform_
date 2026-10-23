using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SorteFederation.Builders.Services;

internal static class PurchaseApprovalRecordRules
{
    public static bool IsRequestObject(string? key) => string.Equals(key, "purchase-requests", StringComparison.OrdinalIgnoreCase);

    public static string ForceDraftStatus(string json, string statusField)
    {
        var data = JsonNode.Parse(json) as JsonObject ?? throw new ArgumentException("Record data must be a JSON object.");
        var actualKey = data.Select(pair => pair.Key).FirstOrDefault(key => key.Equals(statusField, StringComparison.OrdinalIgnoreCase)) ?? statusField;
        if (data.TryGetPropertyValue(actualKey, out var value) && value is not null &&
            !string.Equals(value.ToString(), "Draft", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Purchase requests must be created as Draft. Use the approval submission action to change their status.");
        data[actualKey] = "Draft";
        return data.ToJsonString();
    }

    public static void EnsureStatusUnchanged(string beforeJson, string afterJson, string statusField)
    {
        using var before = JsonDocument.Parse(beforeJson);
        using var after = JsonDocument.Parse(afterJson);
        var beforeStatus = ReadProperty(before.RootElement, statusField)?.ToString() ?? "Draft";
        var afterStatus = ReadProperty(after.RootElement, statusField)?.ToString() ?? "Draft";
        if (!string.Equals(beforeStatus, afterStatus, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Purchase request status is controlled by the approval workflow and cannot be changed through record editing.");
        if (beforeStatus.Equals("In approval", StringComparison.OrdinalIgnoreCase) || beforeStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A request in approval or already approved cannot be edited.");
    }

    public static string SetStatus(string json, string statusField, string status)
    {
        var data = JsonNode.Parse(json) as JsonObject ?? throw new ArgumentException("Purchase request data must be a JSON object.");
        var key = data.Select(pair => pair.Key).FirstOrDefault(name => name.Equals(statusField, StringComparison.OrdinalIgnoreCase)) ?? statusField;
        data[key] = status;
        return data.ToJsonString();
    }

    public static string? ReadProperty(JsonElement data, string name)
    {
        if (data.ValueKind != JsonValueKind.Object) return null;
        foreach (var property in data.EnumerateObject())
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return property.Value.ValueKind == JsonValueKind.Null ? null : property.Value.ToString();
        return null;
    }

    public static string? ActorId(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? user.FindFirstValue("sub") ?? user.FindFirstValue("user_id") ?? user.Identity?.Name;

    public static string? FindField(JsonElement data, string name) => ReadProperty(data, name);
}
