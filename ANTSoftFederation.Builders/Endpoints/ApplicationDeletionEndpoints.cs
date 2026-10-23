using SorteFederation.Builders.Data.Domain;
using SorteFederation.GlobalEnums;
using DefinitionStatus = SorteFederation.Builders.Data.Enums.DefinitionStatus;
using System.Security.Claims;

namespace SorteFederation.Builders.Endpoints;

public sealed class ApplicationDeletionEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapApplicationDeletionEndpoints(endpoints);

    public static IEndpointRouteBuilder MapApplicationDeletionEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/application-deletion-requests", async (Guid? tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!IsAdmin(user)) return Results.Forbid();
            var query = db.ApplicationDeletionRequests.AsNoTracking().Where(x => x.Status == ApplicationDeletionRequestStatus.Pending);
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin))
            {
                if (!Guid.TryParse(TenantClaim(user), out var currentTenant) || currentTenant != tenantId) return Results.Forbid();
                query = query.Where(x => x.TenantId == currentTenant);
            }
            else if (tenantId is not null) query = query.Where(x => x.TenantId == tenantId);
            return Results.Ok(await query.Join(db.Applications.IgnoreQueryFilters(), request => request.ApplicationId, app => app.Id,
                (request, app) => new { request.Id, request.TenantId, request.ApplicationId, ApplicationName = app.Name, app.Status, request.RequestedByUserId, request.Reason, request.CreatedAt })
                .OrderBy(x => x.CreatedAt).ToListAsync(token));
        }).RequireAuthorization().WithTags("Application governance");

        endpoints.MapPost("/applications/{applicationId:guid}/deletion-requests", async (Guid applicationId, CreateApplicationDeletionRequest body, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            var tenant = GetTenant(user, body.TenantId);
            if (tenant is null || !Guid.TryParse(tenant, out var tenantId)) return Results.Forbid();
            var userId = ApplicationEditingPolicy.UserId(user);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Forbid();
            var app = await db.Applications.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId && x.RecordState == RecordState.Active, token);
            if (app is null) return Results.NotFound();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Only unpublished applications can be submitted for deletion.");
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(body.Details)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["details"] = ["Application details are required for administrator review."] });
            var reason = body.Details.Trim();
            if (reason.Length > 2000) return Results.ValidationProblem(new Dictionary<string, string[]> { ["details"] = ["Details must be 2000 characters or fewer."] });
            if (await db.ApplicationDeletionRequests.AnyAsync(x => x.ApplicationId == applicationId && x.Status == ApplicationDeletionRequestStatus.Pending, token))
                return Results.Conflict("A deletion request is already pending for this application.");
            var request = new BuilderApplicationDeletionRequest { TenantId = tenantId, ApplicationId = app.Id, RequestedByUserId = userId, Reason = reason };
            db.ApplicationDeletionRequests.Add(request);
            db.AuditEvents.Add(new BuilderAuditEvent { TenantId = tenantId, EventType = "ApplicationDeletionRequested", ResourceType = "BuilderApplication", ResourceId = app.Id, ActorId = userId, DataJson = System.Text.Json.JsonSerializer.Serialize(new { request.Id, request.Reason }) });
            await db.SaveChangesAsync(token);
            return Results.Created($"/application-deletion-requests/{request.Id}", new { request.Id, request.ApplicationId, request.Status, request.Reason, request.CreatedAt });
        }).RequireAuthorization().WithTags("Application governance");

        endpoints.MapPost("/application-deletion-requests/{requestId:guid}/review", async (Guid requestId, ReviewApplicationDeletionRequest body, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!IsAdmin(user)) return Results.Forbid();
            var reviewerId = ApplicationEditingPolicy.UserId(user);
            if (string.IsNullOrWhiteSpace(reviewerId)) return Results.Forbid();
            var request = await db.ApplicationDeletionRequests.SingleOrDefaultAsync(x => x.Id == requestId && x.Status == ApplicationDeletionRequestStatus.Pending, token);
            if (request is null) return Results.NotFound();
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && request.TenantId != GetTenantId(user)) return Results.Forbid();
            // System administrators may approve their own deletion requests;
            // tenant administrators remain subject to separation of duties.
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && request.RequestedByUserId == reviewerId)
                return Results.Forbid();
            var app = await db.Applications.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token);
            if (app is null || app.Status != DefinitionStatus.Draft || app.RecordState != RecordState.Active) return Results.Conflict("This application is no longer eligible for deletion.");
            request.ReviewedByUserId = reviewerId;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            request.ReviewReason = string.IsNullOrWhiteSpace(body.Reason) ? null : body.Reason.Trim();
            if (body.Approve)
            {
                app.RecordState = RecordState.Deleted;
                app.UpdatedAt = DateTimeOffset.UtcNow;
                app.UpdatedByUserId = reviewerId;
                request.Status = ApplicationDeletionRequestStatus.Approved;
            }
            else request.Status = ApplicationDeletionRequestStatus.Rejected;
            db.AuditEvents.Add(new BuilderAuditEvent { TenantId = request.TenantId, EventType = body.Approve ? "ApplicationDeletionApproved" : "ApplicationDeletionRejected", ResourceType = "BuilderApplication", ResourceId = app.Id, ActorId = reviewerId, DataJson = System.Text.Json.JsonSerializer.Serialize(new { request.Id, request.Reason, request.ReviewReason }) });
            await db.SaveChangesAsync(token);
            return Results.Ok(new { request.Id, request.ApplicationId, request.Status, app.RecordState, request.ReviewedAt });
        }).RequireAuthorization().WithTags("Application governance");
        return endpoints;
    }

    private static bool IsAdmin(ClaimsPrincipal user) => user.IsInRole(BuilderRoleNames.SystemAdmin) || user.IsInRole(BuilderRoleNames.TenantAdmin);
    private static string? TenantClaim(ClaimsPrincipal user) => user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId");
    private static Guid GetTenantId(ClaimsPrincipal user) => Guid.TryParse(TenantClaim(user), out var id) ? id : Guid.Empty;
    private static string? GetTenant(ClaimsPrincipal user, Guid requested) => user.IsInRole(BuilderRoleNames.SystemAdmin) ? requested.ToString() : GetTenantId(user) == requested ? requested.ToString() : null;
}

public sealed record CreateApplicationDeletionRequest(Guid TenantId, string Details);
public sealed record ReviewApplicationDeletionRequest(bool Approve, string? Reason = null);
