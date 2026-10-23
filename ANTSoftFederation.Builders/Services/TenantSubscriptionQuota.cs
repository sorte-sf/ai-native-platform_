namespace SorteFederation.Builders.Services;

internal static class TenantSubscriptionQuota
{
    public static async Task EnsureCanCreateApplication(BuilderDbContext db, Guid tenantId, CancellationToken token, bool bypassSubscription = false)
    {
        if (bypassSubscription) return;
        var limits = await GetActiveLimits(db, tenantId, token);
        var count = await db.Applications.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantId &&
            (x.RecordState == RecordState.Active || x.RecordState == RecordState.Passive), token);
        if (count >= limits.MaxApplications)
            throw new InvalidOperationException($"The tenant has reached its application limit ({limits.MaxApplications}). Upgrade or renew the subscription to create another application.");
    }

    public static async Task EnsureCanCreateUser(BuilderDbContext db, Guid tenantId, CancellationToken token)
    {
        var limits = await GetActiveLimits(db, tenantId, token);
        var count = await db.ApplicationUsers.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantId && x.IsActive && x.RecordState == RecordState.Active, token);
        if (count >= limits.MaxUsers)
            throw new InvalidOperationException($"The tenant has reached its user limit ({limits.MaxUsers}). Upgrade or renew the subscription to add another user.");
    }

    private static async Task<(int MaxApplications, int MaxUsers)> GetActiveLimits(BuilderDbContext db, Guid tenantId, CancellationToken token)
    {
        var tenant = await db.Tenants.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId, token);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (tenant is null || !tenant.HasActiveSubscription || tenant.SubscriptionStartsOn is null || tenant.SubscriptionStartsOn > today ||
            tenant.SubscriptionEndsOn is null || tenant.SubscriptionEndsOn < today || tenant.SubscriptionMaxApplications < 1 || tenant.SubscriptionMaxUsers < 1)
            throw new InvalidOperationException("The tenant does not have an active subscription. Contact your system administrator to assign or renew a plan.");
        return (tenant.SubscriptionMaxApplications, tenant.SubscriptionMaxUsers);
    }
}
