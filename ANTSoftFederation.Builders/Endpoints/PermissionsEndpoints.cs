using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class PermissionsEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    // Permission mutations are exposed only through TenantAccessEndpoints, which derives
    // tenant scope from the authenticated TenantAdmin token rather than caller-supplied IDs.
    public void MapEndpoint(IEndpointRouteBuilder endpoints) { }

public static IEndpointRouteBuilder MapPermissionsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/permissions", (CreatePermissionRequest request, BuilderMetadataService service, CancellationToken token) => service.CreatePermission(request, token)).RequireAuthorization().WithTags("Permissions");
        endpoints.MapPost("/permission-grants", (GrantPermissionRequest request, BuilderMetadataService service, CancellationToken token) => service.GrantPermission(request, token)).RequireAuthorization().WithTags("Permissions");
        endpoints.MapDelete("/permission-grants/{grantId:guid}", async (Guid grantId, Guid tenantId, BuilderMetadataService service, CancellationToken token) => await service.RevokePermission(tenantId, grantId, token) ? Results.NoContent() : Results.NotFound()).RequireAuthorization().WithTags("Permissions");
        endpoints.MapGet("/permission-grants", async (Guid tenantId, Guid? permissionId, BuilderDbContext db, CancellationToken token) =>
            await db.PermissionGrants.AsNoTracking().Where(x => x.TenantId == tenantId && (permissionId == null || x.PermissionId == permissionId)).OrderBy(x => x.SubjectType).ThenBy(x => x.SubjectId).Take(1000).Select(x => new { x.Id, x.PermissionId, x.SubjectType, x.SubjectId, x.CreatedAt }).ToListAsync(token)).RequireAuthorization().WithTags("Permissions");
        return endpoints;
    }
}
