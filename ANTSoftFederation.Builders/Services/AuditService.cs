
using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Services;

public sealed class AuditService(BuilderDbContext db)
{
    public async Task<BuilderAuditEvent> Record(CreateAuditEventRequest request, ClaimsPrincipal user, string? correlationId, CancellationToken token)
    {
        var audit = new BuilderAuditEvent { TenantId=request.TenantId, EventType=request.EventType.Trim(), ResourceType=request.ResourceType.Trim(), ResourceId=request.ResourceId, DataJson=request.DataJson, ActorId=user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name, CorrelationId=correlationId };

        db.AuditEvents.Add(audit); await db.SaveChangesAsync(token); return audit;
    }
}
