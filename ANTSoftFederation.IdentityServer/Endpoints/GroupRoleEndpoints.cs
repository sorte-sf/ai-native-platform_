
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class GroupRoleEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/group-roles")
                      .WithTags("GroupRoles");

        group.MapGet("/", async (IGroupRoleService groupRoleService, [AsParameters] PaginationParams pagination) =>
        {
            var (groupRoles, totalCount) = await groupRoleService.GetGroupRolesAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = groupRoles, TotalCount = totalCount });
        })
        .WithName("GetGroupRoles")
        .Produces<ApiResponse<IEnumerable<GroupRoleListDto>>>();

        group.MapGet("/{groupId:guid}/{roleId:guid}", async (Guid groupId, Guid roleId, IGroupRoleService groupRoleService) =>
        {
            var groupRole = await groupRoleService.GetGroupRoleByIdAsync(groupId, roleId);
            return groupRole != null ? Results.Ok(groupRole) : Results.NotFound();
        })
        .WithName("GetGroupRoleById")
        .Produces<GroupRoleDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateGroupRoleDto createDto, IGroupRoleService groupRoleService) =>
        {
            var result = await groupRoleService.CreateGroupRoleAsync(createDto);
            return result ? Results.Created($"/api/group-roles/{createDto.GroupId}/{createDto.RoleId}", new { GroupId = createDto.GroupId, RoleId = createDto.RoleId }) : Results.BadRequest("Group role already exists");
        })
        .WithName("CreateGroupRole")
        .Accepts<CreateGroupRoleDto>("application/json")
        .Produces<bool>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{groupId:guid}/{roleId:guid}", async (Guid groupId, Guid roleId, UpdateGroupRoleDto updateDto, IGroupRoleService groupRoleService) =>
        {
            var result = await groupRoleService.UpdateGroupRoleAsync(groupId, roleId, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateGroupRole")
        .Accepts<UpdateGroupRoleDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{groupId:guid}/{roleId:guid}", async (Guid groupId, Guid roleId, IGroupRoleService groupRoleService) =>
        {
            var result = await groupRoleService.DeleteGroupRoleAsync(groupId, roleId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteGroupRole")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}