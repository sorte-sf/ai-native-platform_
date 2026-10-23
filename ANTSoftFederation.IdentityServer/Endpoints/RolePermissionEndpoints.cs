
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class RolePermissionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/role-permissions")
                      .WithTags("RolePermissions");

        group.MapGet("/", async (IRolePermissionService rolePermissionService, [AsParameters] PaginationParams pagination) =>
        {
            var (rolePermissions, totalCount) = await rolePermissionService.GetRolePermissionsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = rolePermissions, TotalCount = totalCount });
        })
        .WithName("GetRolePermissions")
        .Produces<ApiResponse<IEnumerable<RolePermissionListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IRolePermissionService rolePermissionService) =>
        {
            var rolePermission = await rolePermissionService.GetRolePermissionByIdAsync(id);
            return rolePermission != null ? Results.Ok(rolePermission) : Results.NotFound();
        })
        .WithName("GetRolePermissionById")
        .Produces<RolePermissionDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateRolePermissionDto createDto, IRolePermissionService rolePermissionService) =>
        {
            var id = await rolePermissionService.CreateRolePermissionAsync(createDto);
            return Results.Created($"/api/role-permissions/{id}", new { Id = id });
        })
        .WithName("CreateRolePermission")
        .Accepts<CreateRolePermissionDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateRolePermissionDto updateDto, IRolePermissionService rolePermissionService) =>
        {
            var result = await rolePermissionService.UpdateRolePermissionAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateRolePermission")
        .Accepts<UpdateRolePermissionDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IRolePermissionService rolePermissionService) =>
        {
            var result = await rolePermissionService.DeleteRolePermissionAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteRolePermission")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}