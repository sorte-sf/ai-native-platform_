using System.Security.Claims;

namespace SorteFederation.Builders.Endpoints;

public sealed record TenantSubscriptionUsageResponse(
    Guid TenantId,
    bool HasActiveSubscription,
    DateOnly? StartsOn,
    DateOnly? EndsOn,
    int ApplicationsUsed,
    int ApplicationsLimit,
    int UsersUsed,
    int UsersLimit,
    int IntegrationsConfigured,
    int AiRequestsThisMonth);

public sealed class TenantSubscriptionUsageEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/subscription/usage", async (
            Guid tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!BuilderApplicationCreationPolicy.CanGenerateForTenant(user, tenantId, out var authorizedTenantId))
                return Results.Forbid();

            var tenant = await db.Tenants.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.TenantId == authorizedTenantId && x.RecordState == RecordState.Active, token);

            if (tenant is null) return Results.NotFound();

            var applicationsUsed = await db.Applications.IgnoreQueryFilters().CountAsync(x =>
                x.TenantId == authorizedTenantId && (x.RecordState == RecordState.Active || x.RecordState == RecordState.Passive), token);

            var usersUsed = await db.ApplicationUsers.IgnoreQueryFilters().CountAsync(x =>
                x.TenantId == authorizedTenantId && x.IsActive && x.RecordState == RecordState.Active, token);

            var integrationsConfigured = await db.TenantIntegrationConfigurations.IgnoreQueryFilters().CountAsync(x =>
                x.TenantId == authorizedTenantId && x.RecordState == RecordState.Active, token);

            var monthStart = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var aiRequestsThisMonth = await db.AiUsage.IgnoreQueryFilters().CountAsync(x =>
                x.TenantId == authorizedTenantId && x.CreatedAt >= monthStart, token);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var hasActiveSubscription = tenant.HasActiveSubscription && tenant.SubscriptionStartsOn <= today &&
                tenant.SubscriptionEndsOn >= today && tenant.SubscriptionMaxApplications > 0 && tenant.SubscriptionMaxUsers > 0;

            return Results.Ok(new TenantSubscriptionUsageResponse(authorizedTenantId, hasActiveSubscription,
                tenant.SubscriptionStartsOn, tenant.SubscriptionEndsOn, applicationsUsed, tenant.SubscriptionMaxApplications,
                usersUsed, tenant.SubscriptionMaxUsers, integrationsConfigured, aiRequestsThisMonth));

        }).RequireAuthorization().WithTags("Subscriptions");
    }
}
