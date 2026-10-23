 
namespace SorteFederation.Builders.Endpoints;

public sealed class AiGovernanceEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/ai").WithTags("AI Governance").RequireAuthorization();

        group.MapGet("/policy", async (Guid tenantId, ClaimsPrincipal user, TenantAiPolicyService policies,
            CancellationToken token) =>
        {
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && !user.IsInRole(BuilderRoleNames.TenantAdmin))
                return Results.Forbid();
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && !OwnsTenant(user, tenantId)) return Results.Forbid();
            var policy = await policies.GetAsync(tenantId, token);
            return Results.Ok(new
            {
                policy.TenantId, policy.Mode, policy.Provider, policy.Endpoint, policy.Model,
                hasApiKey = !string.IsNullOrWhiteSpace(policy.ApiKeyReference),
                policy.MonthlyTokenLimit, policy.MonthlyRequestLimit, policy.MaxOutputTokens
            });
        });

        group.MapPut("/policy", async (UpdateTenantAiPolicyRequest request, ClaimsPrincipal user,
            TenantAiPolicyService policies, BuilderDbContext db, CancellationToken token) =>
        {
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin))
            {
                if (!user.IsInRole(BuilderRoleNames.TenantAdmin) ||
                    !OwnsTenant(user, request.TenantId) ||
                    !request.Mode.Equals("Disabled", StringComparison.OrdinalIgnoreCase) &&
                    !request.Mode.Equals("Local", StringComparison.OrdinalIgnoreCase))
                    return Results.Forbid();
                var existing = await db.AiTenantPolicies.SingleOrDefaultAsync(x => x.TenantId == request.TenantId, token);
                if (existing is null)
                {
                    existing = new BuilderAiTenantPolicy { TenantId = request.TenantId };
                    db.AiTenantPolicies.Add(existing);
                }
                existing.Mode = request.Mode;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                existing.Version++;
                await db.SaveChangesAsync(token);
                return Results.Ok(new { existing.TenantId, existing.Mode, existing.MonthlyTokenLimit, existing.MonthlyRequestLimit, existing.MaxOutputTokens });
            }
            var saved = await policies.SaveAsync(request, token);
            return Results.Ok(new { saved.TenantId, saved.Mode, saved.Provider, saved.Endpoint, saved.Model,
                hasApiKey = !string.IsNullOrWhiteSpace(saved.ApiKeyReference), saved.MonthlyTokenLimit,
                saved.MonthlyRequestLimit, saved.MaxOutputTokens });
        });

        group.MapGet("/usage", async (Guid? tenantId, DateOnly? month, ClaimsPrincipal user,
            BuilderDbContext db, CancellationToken token) =>
        {
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && !user.IsInRole(BuilderRoleNames.TenantAdmin))
                return Results.Forbid();
            if (tenantId is null && !user.IsInRole(BuilderRoleNames.SystemAdmin)) return Results.BadRequest("Tenant ID is required.");
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && tenantId is { } requestedTenant && !OwnsTenant(user, requestedTenant))
                return Results.Forbid();

            var start = month is { } selectedMonth ? new DateOnly(selectedMonth.Year, selectedMonth.Month, 1)
                : new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var end = start.AddMonths(1);
            var query = db.AiUsage.AsNoTracking().Where(x => x.CreatedAt >= start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) &&
                x.CreatedAt < end.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            if (tenantId is { } selectedTenant) query = query.Where(x => x.TenantId == selectedTenant);

            var totalRequests = await query.CountAsync(token);
            var inputTokens = await query.SumAsync(x => (long?)x.InputTokens, token) ?? 0;
            var outputTokens = await query.SumAsync(x => (long?)x.OutputTokens, token) ?? 0;
            var rows = await query.OrderByDescending(x => x.CreatedAt).Take(500)
                .Select(x => new { x.TenantId, x.ApplicationId, x.UserId, x.Feature, x.Provider, x.Model,
                    x.InputTokens, x.OutputTokens, x.IsEstimated, x.Outcome, x.FailureCode,
                    x.DurationMilliseconds, x.CreatedAt })
                .ToListAsync(token);
            return Results.Ok(new { month = start, totalRequests, inputTokens, outputTokens, totalTokens = inputTokens + outputTokens, items = rows });
        });
    }

    private static bool OwnsTenant(ClaimsPrincipal user, Guid tenantId)
    {
        var value = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId");
        return Guid.TryParse(value, out var claimedTenant) && claimedTenant == tenantId;
    }
}
