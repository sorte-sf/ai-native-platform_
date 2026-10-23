
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class ProjectEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects")
                      .WithTags("Projects");

        group.MapGet("/", async (IProjectService projectService, [AsParameters] PaginationParams pagination) =>
        {
            var (projects, totalCount) = await projectService.GetProjectsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = projects, TotalCount = totalCount });
        })
        .WithName("GetProjects")
        .Produces<ApiResponse<IEnumerable<ProjectListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IProjectService projectService) =>
        {
            var project = await projectService.GetProjectByIdAsync(id);
            return project != null ? Results.Ok(project) : Results.NotFound();
        })
        .WithName("GetProjectById")
        .Produces<ProjectDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateProjectDto createDto, IProjectService projectService) =>
        {
            var id = await projectService.CreateProjectAsync(createDto);
            return Results.Created($"/api/projects/{id}", new { Id = id });
        })
        .WithName("CreateProject")
        .Accepts<CreateProjectDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProjectDto updateDto, IProjectService projectService) =>
        {
            var result = await projectService.UpdateProjectAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateProject")
        .Accepts<UpdateProjectDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IProjectService projectService) =>
        {
            var result = await projectService.DeleteProjectAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProject")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}