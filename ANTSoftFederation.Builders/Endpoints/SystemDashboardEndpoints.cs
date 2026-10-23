using System.Security.Claims;
using SorteFederation.Builders.Data;
using SorteFederation.Builders.Data.Domain;
using SorteFederation.Builders.Data.Enums;
using SorteFederation.GlobalEnums;
using Microsoft.Extensions.Caching.Memory;

namespace SorteFederation.Builders.Endpoints;

public static class SystemDashboardEndpoints
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/system/dashboard/summary", async (
            ClaimsPrincipal principal,
            BuilderDbContext db,
            IMemoryCache cache,
            CancellationToken ct) =>
        {
            var systemAdmin = principal.IsInRole(BuilderRoleNames.SystemAdmin);
            var tenantAdmin = principal.IsInRole(BuilderRoleNames.TenantAdmin);
            if (!systemAdmin && !tenantAdmin) return (IResult)Results.Forbid();

            Guid? tenantId = null;
            if (!systemAdmin)
            {
                if (!Guid.TryParse(principal.FindFirstValue("tenant_id") ?? principal.FindFirstValue("tenantId") ??
                    principal.FindFirstValue("company_id") ?? principal.FindFirstValue("companyId"), out var parsedTenantId))
                    return (IResult)Results.Forbid();
                tenantId = parsedTenantId;
            }

            var cacheKey = tenantId.HasValue ? $"system-dashboard:tenant:{tenantId.Value:N}" : "system-dashboard:system";
            if (cache.TryGetValue<DashboardSummaryResponse>(cacheKey, out var cached) && cached is not null)
                return (IResult)Results.Ok(cached);

            var applications = db.Applications.IgnoreQueryFilters().AsNoTracking().AsQueryable();
            var users = db.ApplicationUsers.IgnoreQueryFilters().AsNoTracking().AsQueryable();
            if (tenantId.HasValue)
            {
                applications = applications.Where(x => x.TenantId == tenantId.Value);
                users = users.Where(x => x.TenantId == tenantId.Value);
            }

            var activeApps = applications.Where(x => x.RecordState == RecordState.Active);
            var appTotal = await activeApps.CountAsync(ct);
            var published = await activeApps.CountAsync(x => x.Status == DefinitionStatus.Published, ct);
            var drafts = await activeApps.CountAsync(x => x.Status == DefinitionStatus.Draft, ct);
            var archived = await activeApps.CountAsync(x => x.Status == DefinitionStatus.Archived, ct);
            var passiveApps = await applications.CountAsync(x => x.RecordState == RecordState.Passive, ct);
            var deletedApps = await applications.CountAsync(x => x.RecordState == RecordState.Deleted, ct);
            var activeUsers = await users.CountAsync(x => x.RecordState == RecordState.Active && x.IsActive, ct);
            var newUsers30Days = await users.CountAsync(x => x.RecordState == RecordState.Active && x.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-30), ct);

            var latestApps = await activeApps.OrderByDescending(x => x.UpdatedAt).Take(6)
                .Select(x => new DashboardRecentApplication(x.Id, x.Name, x.Status.ToString(), x.UpdatedAt))
                .ToListAsync(ct);

            IReadOnlyList<DashboardTenantBreakdown> tenantBreakdown = [];
            var tenantCount = 0;
            if (systemAdmin)
            {
                var tenantRows = await db.Tenants.IgnoreQueryFilters().AsNoTracking()
                    .Where(x => x.RecordState != RecordState.Deleted)
                    .Select(x => new { x.TenantId, x.Name })
                    .ToListAsync(ct);
                tenantCount = tenantRows.Count;

                var appGroups = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                    .Where(x => x.RecordState == RecordState.Active)
                    .GroupBy(x => x.TenantId)
                    .Select(g => new { TenantId = g.Key, Total = g.Count(), Published = g.Count(x => x.Status == DefinitionStatus.Published), Drafts = g.Count(x => x.Status == DefinitionStatus.Draft) })
                    .ToListAsync(ct);
                var userGroups = await db.ApplicationUsers.IgnoreQueryFilters().AsNoTracking()
                    .Where(x => x.RecordState == RecordState.Active && x.IsActive)
                    .GroupBy(x => x.TenantId)
                    .Select(g => new { TenantId = g.Key, Total = g.Count() })
                    .ToListAsync(ct);
                var appByTenant = appGroups.ToDictionary(x => x.TenantId);
                var usersByTenant = userGroups.ToDictionary(x => x.TenantId, x => x.Total);

                tenantBreakdown = tenantRows
                    .Select(t => appByTenant.TryGetValue(t.TenantId, out var appStats)
                        ? new DashboardTenantBreakdown(t.TenantId, t.Name, appStats.Total, appStats.Published, appStats.Drafts, usersByTenant.GetValueOrDefault(t.TenantId))
                        : new DashboardTenantBreakdown(t.TenantId, t.Name, 0, 0, 0, usersByTenant.GetValueOrDefault(t.TenantId)))
                    .OrderByDescending(x => x.ApplicationCount)
                    .ThenByDescending(x => x.ActiveUserCount)
                    .ThenBy(x => x.Name)
                    .Take(12)
                    .ToArray();
            }

            var response = new DashboardSummaryResponse(
                systemAdmin ? "system" : "tenant",
                tenantId,
                DateTimeOffset.UtcNow,
                tenantCount,
                new DashboardApplicationSummary(appTotal, published, drafts, archived, passiveApps, deletedApps),
                new DashboardUserSummary(activeUsers, newUsers30Days),
                latestApps,
                tenantBreakdown);

            cache.Set(cacheKey, response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Size = 1
            });
            return (IResult)Results.Ok(response);
        }).RequireAuthorization().WithTags("System dashboard").WithName("GetSystemDashboardSummary");
    }
}

public sealed record DashboardSummaryResponse(
    string Scope,
    Guid? TenantId,
    DateTimeOffset AsOfUtc,
    int TenantCount,
    DashboardApplicationSummary Applications,
    DashboardUserSummary Users,
    IReadOnlyList<DashboardRecentApplication> RecentlyUpdatedApplications,
    IReadOnlyList<DashboardTenantBreakdown> TenantBreakdown);

public sealed record DashboardApplicationSummary(int Total, int Published, int Drafts, int Archived, int Passive, int Deleted);
public sealed record DashboardUserSummary(int Active, int CreatedLast30Days);
public sealed record DashboardRecentApplication(Guid Id, string Name, string Status, DateTimeOffset UpdatedAt);
public sealed record DashboardTenantBreakdown(Guid TenantId, string Name, int ApplicationCount, int PublishedCount, int DraftCount, int ActiveUserCount);
