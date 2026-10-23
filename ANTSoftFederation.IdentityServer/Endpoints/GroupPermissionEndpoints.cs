
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class GroupPermissionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/group-permissions")
                      .WithTags("GroupPermissions");

        group.MapGet("/", async (IGroupPermissionService groupPermissionService, [AsParameters] PaginationParams pagination) =>
        {
            var (groupPermissions, totalCount) = await groupPermissionService.GetGroupPermissionsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = groupPermissions, TotalCount = totalCount });
        })
        .WithName("GetGroupPermissions")
        .Produces<ApiResponse<IEnumerable<GroupPermissionListDto>>>();

        group.MapGet("/{groupId:guid}/{permissionId:guid}", async (Guid groupId, Guid permissionId, IGroupPermissionService groupPermissionService) =>
        {
            var groupPermission = await groupPermissionService.GetGroupPermissionByIdAsync(groupId, permissionId);
            return groupPermission != null ? Results.Ok(groupPermission) : Results.NotFound();
        })
        .WithName("GetGroupPermissionById")
        .Produces<GroupPermissionDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateGroupPermissionDto createDto, IGroupPermissionService groupPermissionService) =>
        {
            var result = await groupPermissionService.CreateGroupPermissionAsync(createDto);
            return result ? Results.Created($"/api/group-permissions/{createDto.GroupId}/{createDto.PermissionId}", new { GroupId = createDto.GroupId, PermissionId = createDto.PermissionId }) : Results.BadRequest("Group permission already exists");
        })
        .WithName("CreateGroupPermission")
        .Accepts<CreateGroupPermissionDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{groupId:guid}/{permissionId:guid}", async (Guid groupId, Guid permissionId, UpdateGroupPermissionDto updateDto, IGroupPermissionService groupPermissionService) =>
        {
            var result = await groupPermissionService.UpdateGroupPermissionAsync(groupId, permissionId, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateGroupPermission")
        .Accepts<UpdateGroupPermissionDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{groupId:guid}/{permissionId:guid}", async (Guid groupId, Guid permissionId, IGroupPermissionService groupPermissionService) =>
        {
            var result = await groupPermissionService.DeleteGroupPermissionAsync(groupId, permissionId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteGroupPermission")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}