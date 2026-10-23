
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class UserPermissionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user-permissions")
                      .WithTags("UserPermissions");

        group.MapGet("/", async (IUserPermissionService userPermissionService, [AsParameters] PaginationParams pagination) =>
        {
            var (userPermissions, totalCount) = await userPermissionService.GetUserPermissionsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = userPermissions, TotalCount = totalCount });
        })
        .WithName("GetUserPermissions")
        .Produces<ApiResponse<IEnumerable<UserPermissionListDto>>>();

        group.MapGet("/{userId:guid}/{permissionId:guid}", async (Guid userId, Guid permissionId, IUserPermissionService userPermissionService) =>
        {
            var userPermission = await userPermissionService.GetUserPermissionByIdAsync(userId, permissionId);
            return userPermission != null ? Results.Ok(userPermission) : Results.NotFound();
        })
        .WithName("GetUserPermissionById")
        .Produces<UserPermissionDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserPermissionDto createDto, IUserPermissionService userPermissionService) =>
        {
            var result = await userPermissionService.CreateUserPermissionAsync(createDto);
            return result ? Results.Created($"/api/user-permissions/{createDto.UserId}/{createDto.PermissionId}", new { UserId = createDto.UserId, PermissionId = createDto.PermissionId }) : Results.BadRequest("User permission already exists");
        })
        .WithName("CreateUserPermission")
        .Accepts<CreateUserPermissionDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{userId:guid}/{permissionId:guid}", async (Guid userId, Guid permissionId, UpdateUserPermissionDto updateDto, IUserPermissionService userPermissionService) =>
        {
            var result = await userPermissionService.UpdateUserPermissionAsync(userId, permissionId, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateUserPermission")
        .Accepts<UpdateUserPermissionDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userId:guid}/{permissionId:guid}", async (Guid userId, Guid permissionId, IUserPermissionService userPermissionService) =>
        {
            var result = await userPermissionService.DeleteUserPermissionAsync(userId, permissionId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUserPermission")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}