
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class UserGroupEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user-groups")
                      .WithTags("UserGroups");

        group.MapGet("/", async (IUserGroupService userGroupService, [AsParameters] PaginationParams pagination) =>
        {
            var (userGroups, totalCount) = await userGroupService.GetUserGroupsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = userGroups, TotalCount = totalCount });
        })
        .WithName("GetUserGroups")
        .Produces<ApiResponse<IEnumerable<UserGroupListDto>>>();

        group.MapGet("/{userId:guid}/{groupId:guid}", async (Guid userId, Guid groupId, IUserGroupService userGroupService) =>
        {
            var userGroup = await userGroupService.GetUserGroupByIdAsync(userId, groupId);
            return userGroup != null ? Results.Ok(userGroup) : Results.NotFound();
        })
        .WithName("GetUserGroupById")
        .Produces<UserGroupDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserGroupDto createDto, IUserGroupService userGroupService) =>
        {
            var result = await userGroupService.CreateUserGroupAsync(createDto);
            return result ? Results.Created($"/api/user-groups/{createDto.UserId}/{createDto.GroupId}", new { UserId = createDto.UserId, GroupId = createDto.GroupId }) : Results.BadRequest("User group already exists");
        })
        .WithName("CreateUserGroup")
        .Accepts<CreateUserGroupDto>("application/json")
        .Produces<bool>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{userId:guid}/{groupId:guid}", async (Guid userId, Guid groupId, UpdateUserGroupDto updateDto, IUserGroupService userGroupService) =>
        {
            var result = await userGroupService.UpdateUserGroupAsync(userId, groupId, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateUserGroup")
        .Accepts<UpdateUserGroupDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userId:guid}/{groupId:guid}", async (Guid userId, Guid groupId, IUserGroupService userGroupService) =>
        {
            var result = await userGroupService.DeleteUserGroupAsync(userId, groupId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUserGroup")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}