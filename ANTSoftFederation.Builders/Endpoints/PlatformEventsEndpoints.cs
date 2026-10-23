using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data.Enums;
using SorteFederation.Builders.Services;
namespace SorteFederation.Builders.Endpoints;
public sealed class PlatformEventsEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapPlatformEventsEndpoints(endpoints);

public static IEndpointRouteBuilder MapPlatformEventsEndpoints(IEndpointRouteBuilder e)
    {
        e.MapPost("/platform-events", async (PublishPlatformEventRequest r, BuilderDbContext db, PlatformEventQueue q, CancellationToken t) =>
        {
            var persisted = new BuilderPlatformEvent { TenantId = r.TenantId, Type = r.Type.Trim(), PayloadJson = r.PayloadJson };
            db.PlatformEvents.Add(persisted);
            await db.SaveChangesAsync(t);
            await q.Enqueue(new PlatformEvent(persisted.Id, persisted.TenantId, persisted.Type, persisted.PayloadJson, persisted.CreatedAt), t);
            persisted.Status = PlatformEventStatus.Queued;
            await db.SaveChangesAsync(t);
            return Results.Accepted(value: new { persisted.Id, persisted.Status });
        }).RequireAuthorization().WithTags("Platform Events");
        e.MapGet("/platform-events", async (Guid tenantId, PlatformEventStatus? status, BuilderDbContext db, CancellationToken t) =>
            await db.PlatformEvents.AsNoTracking()
                .Where(x => x.TenantId == tenantId && (status == null || x.Status == status))
                .OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(t))
            .WithTags("Platform Events");
        e.MapPost("/platform-events/{id:guid}/retry", async (Guid id, Guid tenantId, BuilderDbContext db, PlatformEventQueue q, CancellationToken t) =>
        {
            var value = await db.PlatformEvents.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, t);
            if (value is null) return Results.NotFound();
            if (value.Status != PlatformEventStatus.Failed) return Results.Conflict(new { error = "Only failed events can be retried." });
            if (value.Attempts >= 5) return Results.Conflict(new { error = "Retry limit reached; operator intervention is required." });
            value.Status = PlatformEventStatus.Queued;
            value.Error = null;
            value.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(t);
            await q.Enqueue(new PlatformEvent(value.Id, value.TenantId, value.Type, value.PayloadJson, value.CreatedAt), t);
            return Results.Accepted(value: new { value.Id, value.Status });
        }).WithTags("Platform Events");
        return e;
    }
}
