using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SorteFederation.Builders.Services;

public sealed class FormRuntimeService(BuilderDbContext metadata)
{
    // The client may preview formulas, but persisted values are always derived here.
    public async Task<string> PrepareForWrite(Guid tenantId, Guid objectId, string dataJson, CancellationToken token)
    {
        using var document = JsonDocument.Parse(dataJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Record data must be a JSON object.");
        var values = document.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => (object?)x.Value.Clone(), StringComparer.OrdinalIgnoreCase);
        var fields = await metadata.Fields.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == objectId).ToListAsync(token);
        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field.ConfigurationJson)) continue;
            using var config = JsonDocument.Parse(field.ConfigurationJson);
            if (field.Type == BuilderFieldType.Lookup && config.RootElement.TryGetProperty("lookup", out var lookup))
            {
                if (!values.TryGetValue(field.Key, out var raw) || raw is not JsonElement value || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString())) continue;
                if (value.ValueKind != JsonValueKind.String || !Guid.TryParse(value.GetString(), out var recordId) ||
                    !lookup.TryGetProperty("targetObjectId", out var target) || !Guid.TryParse(target.GetString(), out var targetObjectId) ||
                    !await metadata.Records.AsNoTracking().AnyAsync(x => x.Id == recordId && x.TenantId == tenantId && x.ObjectId == targetObjectId, token))
                    throw new ArgumentException($"Field '{field.Key}' must reference a record in its configured lookup table.");
            }
            if (field.Type == BuilderFieldType.Decimal && config.RootElement.TryGetProperty("formula", out var formula))
            {
                var left = formula.GetProperty("left").GetString()!;
                var right = formula.GetProperty("right").GetString()!;
                var op = formula.GetProperty("operator").GetString();
                if (!values.TryGetValue(left, out var leftValue) || leftValue is not JsonElement leftJson || leftJson.ValueKind != JsonValueKind.Number || !leftJson.TryGetDecimal(out var a) ||
                    !values.TryGetValue(right, out var rightValue) || rightValue is not JsonElement rightJson || rightJson.ValueKind != JsonValueKind.Number || !rightJson.TryGetDecimal(out var b))
                    throw new ArgumentException($"Calculated field '{field.Key}' needs numeric values for '{left}' and '{right}'.");
                try { values[field.Key] = op switch {
                    "+" => checked(a + b), "-" => checked(a - b), "*" => checked(a * b),
                    "/" when b != 0 => a / b,
                    _ => throw new ArgumentException($"Invalid operator or division by zero in calculated field '{field.Key}'.")
                }; }
                catch (OverflowException) { throw new ArgumentException($"Calculated field '{field.Key}' exceeds the supported decimal range."); }
            }
        }
        return JsonSerializer.Serialize(values);
    }

    public async Task<string> FilterReadableAsync(Guid tenantId, Guid objectId, string dataJson, System.Security.Claims.ClaimsPrincipal? user, CancellationToken token)
    {
        if (IsElevated(user)) return dataJson;
        var fields = await metadata.Fields.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == objectId).ToListAsync(token);
        var fieldsByKey = fields.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        var claims = user?.Claims.Where(x => x.Type == "builder_field_read").Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        var hasClaimAllowList = claims.Any(x => x.StartsWith(objectId + ":", StringComparison.OrdinalIgnoreCase));
        var roles = UserRoles(user);
        using var source = JsonDocument.Parse(dataJson);
        var result = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in source.RootElement.EnumerateObject())
        {
            if (hasClaimAllowList && !claims.Contains($"{objectId}:{property.Name}")) continue;
            if (fieldsByKey.TryGetValue(property.Name, out var field) && !HasFieldRoleAccess(field, "readRoles", roles)) continue;
            result[property.Name] = property.Value.Clone();
        }
        return JsonSerializer.Serialize(result);
    }

    public async Task<string> PreserveUnwritableFieldsAsync(Guid tenantId, Guid objectId, string originalJson,
        string incomingJson, System.Security.Claims.ClaimsPrincipal? user, CancellationToken token)
    {
        if (IsElevated(user)) return incomingJson;
        using var original = JsonDocument.Parse(originalJson);
        using var incoming = JsonDocument.Parse(incomingJson);
        if (original.RootElement.ValueKind != JsonValueKind.Object || incoming.RootElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Form data must be a JSON object.");

        var values = incoming.RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.OrdinalIgnoreCase);
        var fields = await metadata.Fields.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ObjectId == objectId).ToListAsync(token);
        var roles = UserRoles(user);
        var claims = user?.Claims.Where(x => x.Type == "builder_field_write").Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        var hasClaimAllowList = claims.Any(value => value.StartsWith(objectId + ":", StringComparison.OrdinalIgnoreCase));

        foreach (var field in fields)
        {
            var roleAllowsWrite = HasFieldRoleAccess(field, "writeRoles", roles);
            var claimAllowsWrite = !hasClaimAllowList || claims.Contains($"{objectId}:{field.Key}");
            if (roleAllowsWrite && claimAllowsWrite) continue;

            var previous = original.RootElement.EnumerateObject()
                .FirstOrDefault(property => property.Name.Equals(field.Key, StringComparison.OrdinalIgnoreCase));
            values.Remove(field.Key);
            if (previous.Name is not null) values[field.Key] = previous.Value.Clone();
        }

        return JsonSerializer.Serialize(values);
    }

    public async Task<FormValidationResponse> Validate(ValidateFormRequest request, System.Security.Claims.ClaimsPrincipal? user, CancellationToken token, string? originalDataJson = null)
    {
        var fields = await metadata.Fields.AsNoTracking().Where(x => x.TenantId == request.TenantId && x.ObjectId == request.ObjectId).ToListAsync(token);
        if (fields.Count == 0) return new(false, ["The object has no published fields or does not belong to this tenant."]);
        using var document = JsonDocument.Parse(request.DataJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object) return new(false, ["Form data must be a JSON object."]);
        var errors = new List<string>();
        var fieldClaims = user?.Claims.Where(x => x.Type == "builder_field_write").Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        var elevated = IsElevated(user);
        var restricted = !elevated && fieldClaims.Any(x => x.StartsWith(request.ObjectId + ":", StringComparison.OrdinalIgnoreCase));
        var roles = UserRoles(user);
        using var originalDocument = originalDataJson is null ? null : JsonDocument.Parse(originalDataJson);
        foreach (var field in fields)
        {
            var present = document.RootElement.TryGetProperty(field.Key, out var value);
            var changed = present && IsChanged(value, originalDocument?.RootElement, field.Key);
            var fieldAllowedByClaim = !restricted || fieldClaims.Contains($"{request.ObjectId}:{field.Key}");
            var fieldAllowedByRole = elevated || HasFieldRoleAccess(field, "writeRoles", roles);
            var calculated = IsCalculated(field);
            if (request.ForWrite && !calculated && present && changed && (!fieldAllowedByClaim || !fieldAllowedByRole))
                errors.Add($"You do not have write permission for field '{field.Key}'.");
            if (request.ForWrite && field.ReadOnly && !calculated && present && changed) errors.Add($"Field '{field.Key}' is read-only.");
            var exists = present && value.ValueKind != JsonValueKind.Null && (value.ValueKind != JsonValueKind.String || !string.IsNullOrWhiteSpace(value.GetString()));
            if (field.Required && !exists) errors.Add($"Field '{field.Key}' is required.");
            if (exists && !IsCompatible(field.Type, value)) errors.Add($"Field '{field.Key}' has an invalid value for type '{field.Type}'.");
            if (exists) ApplyRules(field, value, errors);
        }
        if (request.ForWrite)
        {
            var declared = fields.Select(x => x.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var property in document.RootElement.EnumerateObject())
                if (!declared.Contains(property.Name)) errors.Add($"Field '{property.Name}' is not defined for this object.");
        }
        return new(errors.Count == 0, errors);
    }

    private static bool IsElevated(System.Security.Claims.ClaimsPrincipal? user) =>
        user?.IsInRole(BuilderRoleNames.SystemAdmin) == true || user?.IsInRole(BuilderRoleNames.TenantAdmin) == true;

    public static bool CanReadField(BuilderField field, System.Security.Claims.ClaimsPrincipal? user) =>
        IsElevated(user) || HasFieldRoleAccess(field, "readRoles", UserRoles(user));

    public static bool CanWriteField(BuilderField field, System.Security.Claims.ClaimsPrincipal? user) =>
        IsElevated(user) || HasFieldRoleAccess(field, "writeRoles", UserRoles(user));

    public static bool IsCalculated(BuilderField field) =>
        field.Type == BuilderFieldType.Decimal && field.ConfigurationJson is not null &&
        field.ConfigurationJson.Contains("\"formula\"", StringComparison.Ordinal);

    private static HashSet<string> UserRoles(System.Security.Claims.ClaimsPrincipal? user) =>
        user?.Claims.Where(x => x.Type is System.Security.Claims.ClaimTypes.Role or "role")
            .Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

    private static bool HasFieldRoleAccess(BuilderField field, string accessKey, HashSet<string> roles)
    {
        if (string.IsNullOrWhiteSpace(field.ConfigurationJson)) return true;
        try
        {
            using var config = JsonDocument.Parse(field.ConfigurationJson);
            if (!config.RootElement.TryGetProperty("permissions", out var permissions) ||
                !permissions.TryGetProperty(accessKey, out var allowedRoles) || allowedRoles.ValueKind != JsonValueKind.Array)
                return true;
            var allowed = allowedRoles.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString()!).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return allowed.Count == 0 || roles.Overlaps(allowed);
        }
        catch (JsonException) { return false; }
    }

    private static bool IsChanged(JsonElement value, JsonElement? original, string key)
    {
        if (original is not JsonElement originalValue || originalValue.ValueKind != JsonValueKind.Object) return true;
        var previousProperty = originalValue.EnumerateObject()
            .FirstOrDefault(property => property.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (previousProperty.Name is null) return true;
        var previous = previousProperty.Value;
        return !JsonElement.DeepEquals(value, previous);
    }

    private static bool IsCompatible(BuilderFieldType type, JsonElement value) => type switch
    {
        BuilderFieldType.Integer => value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out _),
        BuilderFieldType.Decimal => value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out _),
        BuilderFieldType.Boolean => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
        BuilderFieldType.Text or BuilderFieldType.LongText or BuilderFieldType.Email or BuilderFieldType.Phone or BuilderFieldType.Url or BuilderFieldType.Choice or BuilderFieldType.MultiChoice or BuilderFieldType.User or BuilderFieldType.Lookup or BuilderFieldType.File => value.ValueKind is JsonValueKind.String or JsonValueKind.Array,
        _ => value.ValueKind != JsonValueKind.Undefined
    };

    private static void ApplyRules(BuilderField field, JsonElement value, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(field.ConfigurationJson)) return;
        using var config = JsonDocument.Parse(field.ConfigurationJson);
        if (value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString() ?? string.Empty;
            if (config.RootElement.TryGetProperty("minLength", out var min) && text.Length < min.GetInt32()) errors.Add($"Field '{field.Key}' is shorter than the minimum length.");
            if (config.RootElement.TryGetProperty("maxLength", out var max) && text.Length > max.GetInt32()) errors.Add($"Field '{field.Key}' exceeds the maximum length.");
            if (config.RootElement.TryGetProperty("pattern", out var pattern) && !System.Text.RegularExpressions.Regex.IsMatch(text, pattern.GetString() ?? string.Empty)) errors.Add($"Field '{field.Key}' has an invalid format.");
        }
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
        {
            if (config.RootElement.TryGetProperty("min", out var min) && number < min.GetDecimal()) errors.Add($"Field '{field.Key}' is below the minimum.");
            if (config.RootElement.TryGetProperty("max", out var max) && number > max.GetDecimal()) errors.Add($"Field '{field.Key}' exceeds the maximum.");
        }
    }
}
