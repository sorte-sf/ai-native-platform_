using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data;
using SorteFederation.Builders.Data.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Claims;

namespace SorteFederation.Builders.Services;

public sealed class DynamicQueryService(TenantRuntimeContextFactory factory, BuilderAuthorizationService authorization, FormRuntimeService forms, BuilderDbContext metadata, IHttpContextAccessor accessor)
{
    public async Task<DynamicQueryResponse> Query(DynamicQueryRequest request, CancellationToken token, ClaimsPrincipal? executionUser = null)
    {
        var page = Math.Max(request.Page, 1); var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var user = executionUser ?? accessor.HttpContext?.User ?? new ClaimsPrincipal();
        
        await authorization.EnsureObjectAccess(request.TenantId, request.ObjectId, "read", user, token);
        await EnsureQueryFieldsAllowed(request, user, token);

        await using var db = await factory.CreateAsync(request.TenantId, token);
        
        var query = db.Records.AsNoTracking().Where(x => x.TenantId == request.TenantId && x.ObjectId == request.ObjectId);
        var allowedIds = await authorization.RecordGrantIds(request.TenantId, request.ObjectId, "read", user, token);
        if (allowedIds is not null) query = query.Where(record => allowedIds.Contains(record.Id));

        // Widget counts and previews must not materialize the whole object's data.
        if ((request.Filters is null || request.Filters.Count == 0) && string.IsNullOrWhiteSpace(request.SortField))
        {
            var total = await query.CountAsync(token);
            var items = await query.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(token);
            foreach (var record in items)
                record.DataJson = await forms.FilterReadableAsync(request.TenantId, record.ObjectId, record.DataJson, user, token);
            return new(items, total, page, pageSize);
        }

        var records = await query.ToListAsync(token);
        var filtered = records.Where(x => Matches(x.DataJson, request.Filters)).ToList();
        if (!string.IsNullOrWhiteSpace(request.SortField)) filtered = filtered.OrderBy(x => Value(x.DataJson, request.SortField!)).ToList();
        if (request.Descending) filtered.Reverse();
        foreach (var record in filtered)
            record.DataJson = await forms.FilterReadableAsync(request.TenantId, record.ObjectId, record.DataJson, user, token);
        return new(filtered.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), filtered.Count, page, pageSize);
    }

    private async Task EnsureQueryFieldsAllowed(DynamicQueryRequest request, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        if (user.IsInRole(BuilderRoleNames.SystemAdmin) || user.IsInRole(BuilderRoleNames.TenantAdmin)) return;
        var restricted = user.Claims.Where(x => x.Type == "builder_field_read" && x.Value.StartsWith(request.ObjectId + ":", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value[(request.ObjectId.ToString().Length + 1)..]).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var fields = request.Filters?.Select(x => x.Field).Append(request.SortField ?? "").Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase) ?? [];
        var selectedFields = fields.ToArray();
        if (selectedFields.Length == 0) return;
        var known = await metadata.Fields.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && x.ObjectId == request.ObjectId && x.Searchable)
            .ToListAsync(token);
        if (selectedFields.Any(x => !known.Any(field => field.Key.Equals(x, StringComparison.OrdinalIgnoreCase))))
            throw new ArgumentException("One or more query fields are not searchable.");
        if (selectedFields.Any(x => restricted.Count > 0 && !restricted.Contains(x) ||
            !FormRuntimeService.CanReadField(known.Single(field => field.Key.Equals(x, StringComparison.OrdinalIgnoreCase)), user)))
            throw new UnauthorizedAccessException("The query references a field that the current user cannot read.");
    }

    private static bool Matches(string json, IReadOnlyList<DynamicFilter>? filters)
    {
        if (filters is null || filters.Count == 0) return true;
        using var document = JsonDocument.Parse(json);
        return filters.All(f => TryGetProperty(document.RootElement, f.Field, out var actual) && Compare(actual, f.Operator, f.Value));
    }

    private static string Value(string json, string field)
    {
        using var d = JsonDocument.Parse(json); return TryGetProperty(d.RootElement, field, out var v) ? v.ToString() : string.Empty;
    }

    private static bool TryGetProperty(JsonElement element, string key, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
            if (property.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        value = default;
        return false;
    }

    private static bool Compare(JsonElement actual, string op, string? expected)
    {
        var actualText = actual.ToString();
        return op.ToLowerInvariant() switch
        {
            "eq" => string.Equals(actualText, expected, StringComparison.OrdinalIgnoreCase),
            "contains" => actualText.Contains(expected ?? string.Empty, StringComparison.OrdinalIgnoreCase),
            "in" => (expected ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Contains(actualText, StringComparer.OrdinalIgnoreCase),
            "gt" => decimal.TryParse(actualText, out var gt) && decimal.TryParse(expected, out var gtExpected) && gt > gtExpected,
            "gte" => decimal.TryParse(actualText, out var gte) && decimal.TryParse(expected, out var gteExpected) && gte >= gteExpected,
            "lt" => decimal.TryParse(actualText, out var lt) && decimal.TryParse(expected, out var ltExpected) && lt < ltExpected,
            "lte" => decimal.TryParse(actualText, out var lte) && decimal.TryParse(expected, out var lteExpected) && lte <= lteExpected,
            _ => false
        };
    }
}
