using SorteFederation.Builders.Services;
using SorteFederation.Builders.Contracts;
using System.Text.Json;
namespace SorteFederation.Builders.Endpoints;
public sealed class ReportingEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapReportingEndpoints(endpoints);

public static IEndpointRouteBuilder MapReportingEndpoints(IEndpointRouteBuilder e)
    {
        e.MapGet("/reports", async (Guid tenantId, BuilderDbContext db, CancellationToken t) =>
            await db.Reports.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.Name).Take(500).ToListAsync(t))
            .RequireAuthorization().WithTags("Reporting");
        e.MapGet("/reports/{reportId:guid}", async (Guid reportId, Guid tenantId, BuilderDbContext db, CancellationToken t) =>
            await db.Reports.AsNoTracking().SingleOrDefaultAsync(x => x.Id == reportId && x.TenantId == tenantId, t) is { } report
                ? Results.Ok(report) : Results.NotFound())
            .RequireAuthorization().WithTags("Reporting");
        e.MapPost("/reports/{reportId:guid}/run", async (Guid reportId, Guid tenantId, BuilderDbContext db, DynamicQueryService query, CancellationToken t) =>
        {
            var report = await db.Reports.AsNoTracking().SingleOrDefaultAsync(x => x.Id == reportId && x.TenantId == tenantId, t);
            if (report is null) return Results.NotFound();
            var definition = ParseDefinition(report.DefinitionJson);
            var result = await query.Query(new DynamicQueryRequest(tenantId, report.ObjectId, definition.Filters, definition.SortField, definition.Descending, definition.Page, definition.PageSize), t);
            return Results.Ok(new { report.Id, report.Key, report.Name, report.DefinitionJson, result.Items, result.Total, result.Page, result.PageSize });
        }).RequireAuthorization().WithTags("Reporting");
        e.MapGet("/reports/{objectId:guid}/run", async (Guid objectId, Guid tenantId, string? field, string? value, string? aggregate, TenantRecordService s, CancellationToken t) =>
        {
            var records = await s.Query(tenantId, objectId, value, t);
            if (!string.IsNullOrWhiteSpace(aggregate))
            {
                var numbers = records.Select(x => ReadNumber(x.DataJson, field)).Where(x => x.HasValue).Select(x => x!.Value).ToArray();
                return aggregate.Trim().ToLowerInvariant() switch
                {
                    "count" => Results.Ok(new { aggregate = "count", value = records.Count }),
                    "sum" => Results.Ok(new { aggregate = "sum", value = numbers.Sum() }),
                    "avg" => Results.Ok(new { aggregate = "avg", value = numbers.Length == 0 ? 0 : numbers.Average() }),
                    "min" => Results.Ok(new { aggregate = "min", value = numbers.Length == 0 ? 0 : numbers.Min() }),
                    "max" => Results.Ok(new { aggregate = "max", value = numbers.Length == 0 ? 0 : numbers.Max() }),
                    _ => Results.BadRequest("aggregate must be count, sum, avg, min, or max.")
                };
            }
            return Results.Ok(records);
        }).RequireAuthorization().WithTags("Reporting");
        return e;
    }

    private static decimal? ReadNumber(string json, string? field)
    {
        if (string.IsNullOrWhiteSpace(field)) return null;
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(field, out var value) && value.TryGetDecimal(out var number) ? number : null;
    }

    private static (IReadOnlyList<DynamicFilter>? Filters, string? SortField, bool Descending, int Page, int PageSize) ParseDefinition(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var filters = root.TryGetProperty("filters", out var values) && values.ValueKind == JsonValueKind.Array
                ? values.EnumerateArray().Where(x => x.TryGetProperty("field", out _) && x.TryGetProperty("operator", out _)).Select(x => new DynamicFilter(x.GetProperty("field").GetString() ?? "", x.GetProperty("operator").GetString() ?? "eq", x.TryGetProperty("value", out var v) ? v.ToString() : null)).ToArray()
                : null;
            var sort = root.TryGetProperty("sortField", out var sortValue) ? sortValue.GetString() : null;
            var descending = root.TryGetProperty("descending", out var desc) && desc.ValueKind == JsonValueKind.True;
            var page = root.TryGetProperty("page", out var pageValue) && pageValue.TryGetInt32(out var p) ? p : 1;
            var size = root.TryGetProperty("pageSize", out var sizeValue) && sizeValue.TryGetInt32(out var s) ? s : 50;
            return (filters, sort, descending, page, size);
        }
        catch (JsonException) { return (null, null, false, 1, 50); }
    }
}
