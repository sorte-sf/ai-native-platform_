using SorteFederation.Builders.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SorteFederation.Builders.Services;

public sealed class FeatureFlagService(BuilderDbContext db)
{
    public async Task<bool> IsEnabledAsync(Guid tenantId, Guid applicationId, string key, ClaimsPrincipal user, CancellationToken token)
    {
        var flag = await db.FeatureFlags.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.Key == key, token);
        if (flag is null || !flag.Enabled) return false;
        if (string.IsNullOrWhiteSpace(flag.RulesJson)) return true;
        using var rules = JsonDocument.Parse(flag.RulesJson);
        if (!rules.RootElement.TryGetProperty("claim", out var claim) || !rules.RootElement.TryGetProperty("value", out var value)) return false;
        return user.Claims.Any(x => x.Type.Equals(claim.GetString(), StringComparison.OrdinalIgnoreCase) && x.Value.Equals(value.GetString(), StringComparison.OrdinalIgnoreCase));
    }
}
