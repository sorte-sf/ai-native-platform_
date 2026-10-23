using System.Security.Claims;

namespace SorteFederation.ANTApplications.Services;

public static class RoleNames
{
    public const string SystemAdmin = "SystemAdmin";
    public const string Admin = "Admin";
    public const string PortalUser = "Portal User";
    public const string PortalDeveloper = "Portal Developer";
    public const string Designer = "Designer";
    public const string TenantAdmin = "Tenant Admin";
    public const string Integrator = "Integrator";
    public const string User = "User";

    private static readonly string[] SuperAdminAliases = { SystemAdmin, "SystemAdmin", "systemAdmin"};
    private static readonly string[] AdminAliases = { Admin, TenantAdmin, "TenantAdmin" };
    private static readonly string[] HiddenLegacyRoles = {"SystemAdmin", "systemAdmin", "TenantAdmin"};
    public static bool IsSuperAdmin(ClaimsPrincipal user) => IsSuperAdminRole(user.FindFirstValue(ClaimTypes.Role));
    public static bool IsTenantAdmin(ClaimsPrincipal user) => IsTenantAdminRole(user.FindFirstValue(ClaimTypes.Role));
    public static bool IsAdmin(ClaimsPrincipal user) => IsSuperAdmin(user) || IsTenantAdmin(user);
    public static bool IsSuperAdminRole(string? role) => SuperAdminAliases.Any(alias => string.Equals(alias, role, StringComparison.OrdinalIgnoreCase));
    public static bool IsTenantAdminRole(string? role) => AdminAliases.Any(alias => string.Equals(alias, role, StringComparison.OrdinalIgnoreCase));
    public static bool CanAssignRole(string role, ClaimsPrincipal user) => IsSuperAdmin(user) || (!IsSuperAdminRole(role) && !IsTenantAdminRole(role) && !IsHiddenLegacyRole(role));
    public static bool IsHiddenLegacyRole(string? role) => HiddenLegacyRoles.Any(alias => string.Equals(alias, role, StringComparison.OrdinalIgnoreCase));
}
