
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class GroupEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/groups")
                      .WithTags("Groups");

        group.MapGet("/", async (IGroupService groupService, [AsParameters] PaginationParams pagination) =>
        {
            var (groups, totalCount) = await groupService.GetGroupsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = groups, TotalCount = totalCount });
        })
        .WithName("GetGroups")
        .Produces<ApiResponse<IEnumerable<GroupListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IGroupService groupService) =>
        {
            var group = await groupService.GetGroupByIdAsync(id);
            return group != null ? Results.Ok(group) : Results.NotFound();
        })
        .WithName("GetGroupById")
        .Produces<GroupDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateGroupDto createDto, IGroupService groupService) =>
        {
            var id = await groupService.CreateGroupAsync(createDto);
            return Results.Created($"/api/groups/{id}", new { Id = id });
        })
        .WithName("CreateGroup")
        .Accepts<CreateGroupDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateGroupDto updateDto, IGroupService groupService) =>
        {
            var result = await groupService.UpdateGroupAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateGroup")
        .Accepts<UpdateGroupDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IGroupService groupService) =>
        {
            var result = await groupService.DeleteGroupAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteGroup")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}