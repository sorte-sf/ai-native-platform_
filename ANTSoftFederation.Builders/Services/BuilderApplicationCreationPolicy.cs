using System.Security.Claims;

namespace SorteFederation.Builders.Services;

/// <summary>Authorization shared by regular and AI-assisted application creation.</summary>
public static class BuilderApplicationCreationPolicy
{
    public static bool CanGenerateForTenant(ClaimsPrincipal user, Guid requestedTenantId, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        if (user.Identity?.IsAuthenticated != true || requestedTenantId == Guid.Empty) return false;

        // A system administrator may select a target tenant. Tenant users may only
        // act for the tenant established by their signed identity claims.
        if (user.IsInRole(BuilderRoleNames.SystemAdmin))
        {
            tenantId = requestedTenantId;
            return true;
        }

        if (!TryGetClaimTenant(user, out var claimTenant) || claimTenant != requestedTenantId) return false;
        tenantId = claimTenant;
        return true;
    }

    public static bool CanCreateForTenant(ClaimsPrincipal user, Guid requestedTenantId, out Guid tenantId)
    {
        if (!CanGenerateForTenant(user, requestedTenantId, out tenantId)) return false;
        return user.IsInRole(BuilderRoleNames.SystemAdmin) || user.IsInRole(BuilderRoleNames.TenantAdmin) ||
            user.IsInRole(BuilderRoleNames.Developer);
    }

    private static bool TryGetClaimTenant(ClaimsPrincipal user, out Guid tenantId) =>
        Guid.TryParse(user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ??
            user.FindFirstValue("companyId"), out tenantId);
}
