 
namespace SorteFederation.Builders.Endpoints;
public sealed class DashboardRuntimeEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapDashboardRuntimeEndpoints(endpoints);

public static IEndpointRouteBuilder MapDashboardRuntimeEndpoints(IEndpointRouteBuilder e)
    {
        e.MapGet("/dashboards/{dashboardId:guid}/runtime", async (Guid dashboardId, Guid tenantId, BuilderDbContext db, TenantRecordService records, CancellationToken token) =>
        {
            var dashboard = await db.Dashboards.AsNoTracking().SingleOrDefaultAsync(x => x.Id == dashboardId && x.TenantId == tenantId, token);
            if (dashboard is null) return Results.NotFound();
            var definitions = await db.Objects.AsNoTracking().Where(x => x.ApplicationId == dashboard.ApplicationId && x.TenantId == tenantId).Select(x => new { x.Id, x.Key, x.Name }).ToListAsync(token);
            var objects = new List<object>();
            foreach (var definition in definitions)
            {
                var count = (await records.Query(tenantId, definition.Id, null, token)).Count;
                objects.Add(new { definition.Id, definition.Key, definition.Name, RecordCount = count });
            }
            var widgets = await db.DashboardWidgets.AsNoTracking().Where(x => x.DashboardId == dashboard.Id && x.TenantId == tenantId).OrderBy(x => x.SortOrder).Select(x => new { x.Id, x.Key, x.Type, x.ConfigurationJson, x.SortOrder }).ToListAsync(token);
            var renderedWidgets = new List<object>();
            foreach (var widget in widgets)
            {
                if (widget.Type.Equals("record-count", StringComparison.OrdinalIgnoreCase) && TryObjectId(widget.ConfigurationJson, out var objectId))
                {
                    var recordsForWidget = await records.Query(tenantId, objectId, null, token);
                    renderedWidgets.Add(new { widget.Id, widget.Key, widget.Type, widget.SortOrder, Value = recordsForWidget.Count });
                }
                else if (new[] { "sum", "average", "min", "max" }.Contains(widget.Type, StringComparer.OrdinalIgnoreCase) && TryMetric(widget.ConfigurationJson, out var metricObjectId, out var field))
                {
                    var values = (await records.Query(tenantId, metricObjectId, null, token)).Select(x => ReadNumber(x.DataJson, field)).Where(x => x.HasValue).Select(x => x!.Value).ToArray();
                    var result = widget.Type.Equals("sum", StringComparison.OrdinalIgnoreCase) ? values.Sum() : widget.Type.Equals("average", StringComparison.OrdinalIgnoreCase) ? values.Length == 0 ? 0 : values.Average() : widget.Type.Equals("min", StringComparison.OrdinalIgnoreCase) ? values.Length == 0 ? 0 : values.Min() : values.Length == 0 ? 0 : values.Max();
                    renderedWidgets.Add(new { widget.Id, widget.Key, widget.Type, widget.SortOrder, Value = result });
                }
                else
                    renderedWidgets.Add(new { widget.Id, widget.Key, widget.Type, widget.SortOrder, Value = (object?)null });
            }
            return Results.Ok(new { dashboard.Id, dashboard.Key, dashboard.Name, dashboard.LayoutJson, objects, widgets, renderedWidgets });
        }).RequireAuthorization().WithTags("Dashboard Runtime");
        return e;
    }

    private static bool TryObjectId(string configurationJson, out Guid objectId)
    {
        objectId = Guid.Empty;
        try
        {
            using var document = JsonDocument.Parse(configurationJson);
            return document.RootElement.TryGetProperty("objectId", out var value) && Guid.TryParse(value.GetString(), out objectId);
        }
        catch (JsonException) { return false; }
    }

    private static bool TryMetric(string configurationJson, out Guid objectId, out string field)
    {
        objectId = Guid.Empty; field = string.Empty;
        try
        {
            using var document = JsonDocument.Parse(configurationJson);
            if (!document.RootElement.TryGetProperty("objectId", out var id) || !Guid.TryParse(id.GetString(), out objectId) || !document.RootElement.TryGetProperty("field", out var name)) return false;
            var parsedField = name.GetString();
            if (string.IsNullOrWhiteSpace(parsedField)) return false;
            field = parsedField;
            return true;
        }
        catch (JsonException) { return false; }
    }

    private static decimal? ReadNumber(string json, string field)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(field, out var value) && value.TryGetDecimal(out var number) ? number : null;
    }
}
