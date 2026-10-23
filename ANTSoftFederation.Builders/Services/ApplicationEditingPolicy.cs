namespace SorteFederation.Builders.Services;

public static class ApplicationEditingPolicy
{
    public static string? UserId(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? user.FindFirstValue("sub") ?? user.FindFirstValue("user_id");
    public static bool CanEdit(ClaimsPrincipal user, BuilderApplication app)
    {
        if (user.Identity?.IsAuthenticated != true) return false;
        if (user.IsInRole(BuilderRoleNames.SystemAdmin)) return true;
        if (!Guid.TryParse(user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id")
            ?? user.FindFirstValue("companyId"), out var tenant) || tenant != app.TenantId) return false;
        return user.IsInRole(BuilderRoleNames.TenantAdmin) || HasWorkspaceRole(user, "BuilderAdmin", "BuilderEditor")
            || (!string.IsNullOrEmpty(app.CreatedByUserId) && app.CreatedByUserId == UserId(user));
    }

    public static bool CanView(ClaimsPrincipal user, BuilderApplication app)
    {
        if (user.Identity?.IsAuthenticated != true) return false;
        if (user.IsInRole(BuilderRoleNames.SystemAdmin)) return true;
        if (!Guid.TryParse(user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id")
            ?? user.FindFirstValue("companyId"), out var tenant) || tenant != app.TenantId) return false;
        return user.IsInRole(BuilderRoleNames.TenantAdmin) || HasWorkspaceRole(user, "BuilderAdmin", "BuilderEditor", "BuilderViewer")
            || (!string.IsNullOrEmpty(app.CreatedByUserId) && app.CreatedByUserId == UserId(user));
    }

    public static bool HasWorkspaceRole(ClaimsPrincipal user, params string[] roles) =>
        roles.Contains(user.FindFirstValue("builder_workspace_role"), StringComparer.OrdinalIgnoreCase);
}
