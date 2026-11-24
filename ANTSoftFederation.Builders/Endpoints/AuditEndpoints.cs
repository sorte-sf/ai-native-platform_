 
namespace SorteFederation.Builders.Endpoints;

public sealed class AuditEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapAuditEndpoints(endpoints);

public static IEndpointRouteBuilder MapAuditEndpoints(IEndpointRouteBuilder e)
    {
        e.MapPost("/audit-events", (CreateAuditEventRequest r, ClaimsPrincipal u, HttpContext c, AuditService s, CancellationToken t) => s.Record(r, u, c.TraceIdentifier, t)).RequireAuthorization().WithTags("Audit");

        e.MapGet("/audit-events", async (Guid tenantId, string? eventType, Guid? resourceId, int? limit, BuilderDbContext db, CancellationToken t) =>
            await db.AuditEvents.AsNoTracking()
                .Where(x => x.TenantId == tenantId && (eventType == null || x.EventType == eventType) && (resourceId == null || x.ResourceId == resourceId))
                .OrderByDescending(x => x.CreatedAt).Take(Math.Clamp(limit ?? 100, 1, 500)).ToListAsync(t))
            .RequireAuthorization().WithTags("Audit");

        return e;
    }
}
