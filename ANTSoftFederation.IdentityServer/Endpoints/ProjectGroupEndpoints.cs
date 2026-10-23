
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class ProjectGroupEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/project-groups")
                      .WithTags("ProjectGroups");

        group.MapGet("/", async (IProjectGroupService projectGroupService, [AsParameters] PaginationParams pagination) =>
        {
            var (projectGroups, totalCount) = await projectGroupService.GetProjectGroupsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = projectGroups, TotalCount = totalCount });
        })
        .WithName("GetProjectGroups")
        .Produces<ApiResponse<IEnumerable<ProjectGroupListDto>>>();

        group.MapGet("/{projectId:guid}/{groupId:guid}", async (Guid projectId, Guid groupId, IProjectGroupService projectGroupService) =>
        {
            var projectGroup = await projectGroupService.GetProjectGroupByIdAsync(projectId, groupId);
            return projectGroup != null ? Results.Ok(projectGroup) : Results.NotFound();
        })
        .WithName("GetProjectGroupById")
        .Produces<ProjectGroupDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateProjectGroupDto createDto, IProjectGroupService projectGroupService) =>
        {
            var result = await projectGroupService.CreateProjectGroupAsync(createDto);
            return result ? Results.Created($"/api/project-groups/{createDto.ProjectId}/{createDto.GroupId}", new { ProjectId = createDto.ProjectId, GroupId = createDto.GroupId }) : Results.BadRequest("Project group already exists");
        })
        .WithName("CreateProjectGroup")
        .Accepts<CreateProjectGroupDto>("application/json")
        .Produces<bool>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{projectId:guid}/{groupId:guid}", async (Guid projectId, Guid groupId, UpdateProjectGroupDto updateDto, IProjectGroupService projectGroupService) =>
        {
            var result = await projectGroupService.UpdateProjectGroupAsync(projectId, groupId, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateProjectGroup")
        .Accepts<UpdateProjectGroupDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{projectId:guid}/{groupId:guid}", async (Guid projectId, Guid groupId, IProjectGroupService projectGroupService) =>
        {
            var result = await projectGroupService.DeleteProjectGroupAsync(projectId, groupId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProjectGroup")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}