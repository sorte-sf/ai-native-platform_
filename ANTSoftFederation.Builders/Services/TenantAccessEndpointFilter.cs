using System.Reflection;
using System.Security.Claims;
using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Services;

/// Prevents a tenant user from submitting a different tenant id to any Builders endpoint.
public sealed class TenantAccessEndpointFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        var workspaceRole = user.FindFirstValue("builder_workspace_role");
        var isWorkspaceEditor = string.Equals(workspaceRole, "BuilderAdmin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(workspaceRole, "BuilderEditor", StringComparison.OrdinalIgnoreCase);
        var isWorkspaceAdmin = string.Equals(workspaceRole, "BuilderAdmin", StringComparison.OrdinalIgnoreCase);
        if (user.HasClaim("builder_workspace_user", "true") &&
            context.HttpContext.Request.Method is "POST" or "PUT" or "PATCH" or "DELETE" && !RequiresDesignerRole(context.HttpContext))
            return ValueTask.FromResult<object?>(Results.Forbid());
        if (RequiresDesignerRole(context.HttpContext) &&
            !user.IsInRole(BuilderRoleNames.SystemAdmin) &&
            !user.IsInRole(BuilderRoleNames.TenantAdmin) &&
            !user.IsInRole(BuilderRoleNames.Developer) && !isWorkspaceEditor)
            return ValueTask.FromResult<object?>(Results.Forbid());

        if (RequiresGovernanceRole(context.HttpContext) &&
            !user.IsInRole(BuilderRoleNames.SystemAdmin) &&
            !user.IsInRole(BuilderRoleNames.TenantAdmin) &&
            !(HttpMethods.IsGet(context.HttpContext.Request.Method) && (isWorkspaceEditor || workspaceRole == "BuilderViewer")) &&
            !(isWorkspaceAdmin && context.HttpContext.Request.Method is "POST" or "PUT" or "PATCH"))
            return ValueTask.FromResult<object?>(Results.Forbid());

        // Deletion review endpoints resolve the request's tenant from the stored request
        // and enforce it in the endpoint itself; there is no client-supplied tenant id.
        if (context.HttpContext.Request.Path.StartsWithSegments("/api/v1/application-deletion-requests") &&
            user.IsInRole(BuilderRoleNames.TenantAdmin))
            return next(context);

        if (user.IsInRole(BuilderRoleNames.SystemAdmin)) return next(context);

        // Templates are global read-only choices for tenant application designers.
        // Creation and mutation are restricted to SystemAdmin on their endpoints.
        if (context.HttpContext.Request.Method == "GET" &&
            context.HttpContext.Request.Path.StartsWithSegments("/api/v1/templates"))
            return next(context);

        var tenantId = FindTenantId(context);
        var claim = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId");
        if (tenantId is null || !Guid.TryParse(claim, out var claimedTenant) || claimedTenant != tenantId)
            return ValueTask.FromResult<object?>(Results.Forbid());

        return next(context);
    }

    private static bool RequiresDesignerRole(HttpContext http) =>
        http.Request.Method is "POST" or "PUT" or "PATCH" or "DELETE" &&
        (http.Request.Path.StartsWithSegments("/api/v1/applications") ||
         http.Request.Path.StartsWithSegments("/api/v1/objects") ||
         http.Request.Path.StartsWithSegments("/api/v1/designer") ||
         http.Request.Path.StartsWithSegments("/api/v1/application-modules") ||
         http.Request.Path.StartsWithSegments("/api/v1/application-menu") ||
         http.Request.Path.StartsWithSegments("/api/v1/fields") ||
         http.Request.Path.StartsWithSegments("/api/v1/forms") ||
         http.Request.Path.StartsWithSegments("/api/v1/relationships") ||
         http.Request.Path.StartsWithSegments("/api/v1/workflows") ||
         http.Request.Path.StartsWithSegments("/api/v1/connectors") ||
         http.Request.Path.StartsWithSegments("/api/v1/rules") ||
         http.Request.Path.StartsWithSegments("/api/v1/reports") ||
         http.Request.Path.StartsWithSegments("/api/v1/dashboards") ||
         http.Request.Path.StartsWithSegments("/api/v1/dashboard-widgets") ||
         http.Request.Path.StartsWithSegments("/api/v1/permissions") ||
         http.Request.Path.StartsWithSegments("/api/v1/connector-credentials") ||
         http.Request.Path.StartsWithSegments("/api/v1/feature-flags") ||
         http.Request.Path.StartsWithSegments("/api/v1/scheduled-jobs") ||
         http.Request.Path.StartsWithSegments("/api/v1/platform-events"));

    private static bool RequiresGovernanceRole(HttpContext http) =>
        http.Request.Path.StartsWithSegments("/api/v1/application-versions") ||
        http.Request.Path.StartsWithSegments("/api/v1/application-deletion-requests") ||
        http.Request.Path.StartsWithSegments("/api/v1/publishing");

    private static Guid? FindTenantId(EndpointFilterInvocationContext context)
    {
        if (Guid.TryParse(context.HttpContext.Request.Query["tenantId"], out var queryTenant)) return queryTenant;
        foreach (var argument in context.Arguments)
        {
            if (argument is null) continue;
            var property = argument.GetType().GetProperty("TenantId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property?.PropertyType == typeof(Guid) && property.GetValue(argument) is Guid id) return id;
        }
        return null;
    }
}
