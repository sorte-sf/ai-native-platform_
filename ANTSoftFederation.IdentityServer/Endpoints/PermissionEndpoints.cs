
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class PermissionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/permissions")
                      .WithTags("Permissions");

        group.MapGet("/", async (IPermissionService permissionService, [AsParameters] PaginationParams pagination) =>
        {
            var (permissions, totalCount) = await permissionService.GetPermissionsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = permissions, TotalCount = totalCount });
        })
        .WithName("GetPermissions")
        .Produces<ApiResponse<IEnumerable<PermissionListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IPermissionService permissionService) =>
        {
            var permission = await permissionService.GetPermissionByIdAsync(id);
            return permission != null ? Results.Ok(permission) : Results.NotFound();
        })
        .WithName("GetPermissionById")
        .Produces<PermissionDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreatePermissionDto createDto, IPermissionService permissionService) =>
        {
            var id = await permissionService.CreatePermissionAsync(createDto);
            return Results.Created($"/api/permissions/{id}", new { Id = id });
        })
        .WithName("CreatePermission")
        .Accepts<CreatePermissionDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdatePermissionDto updateDto, IPermissionService permissionService) =>
        {
            var result = await permissionService.UpdatePermissionAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdatePermission")
        .Accepts<UpdatePermissionDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IPermissionService permissionService) =>
        {
            var result = await permissionService.DeletePermissionAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeletePermission")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}