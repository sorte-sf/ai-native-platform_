
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class UserProjectEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user-projects")
                      .WithTags("UserProjects");

        group.MapGet("/", async (IUserProjectService userProjectService, [AsParameters] PaginationParams pagination) =>
        {
            var (userProjects, totalCount) = await userProjectService.GetUserProjectsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = userProjects, TotalCount = totalCount });
        })
        .WithName("GetUserProjects")
        .Produces<ApiResponse<IEnumerable<UserProjectListDto>>>();

        group.MapGet("/{userId:guid}/{projectId:guid}", async (Guid userId, Guid projectId, IUserProjectService userProjectService) =>
        {
            var userProject = await userProjectService.GetUserProjectByIdAsync(userId, projectId);
            return userProject != null ? Results.Ok(userProject) : Results.NotFound();
        })
        .WithName("GetUserProjectById")
        .Produces<UserProjectDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserProjectDto createDto, IUserProjectService userProjectService) =>
        {
            var result = await userProjectService.CreateUserProjectAsync(createDto);
            return result ? Results.Created($"/api/user-projects/{createDto.UserId}/{createDto.ProjectId}", new { UserId = createDto.UserId, ProjectId = createDto.ProjectId }) : Results.BadRequest("User project already exists");
        })
        .WithName("CreateUserProject")
        .Accepts<CreateUserProjectDto>("application/json")
        .Produces<bool>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{userId:guid}/{projectId:guid}", async (Guid userId, Guid projectId, UpdateUserProjectDto updateDto, IUserProjectService userProjectService) =>
        {
            var result = await userProjectService.UpdateUserProjectAsync(userId, projectId, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateUserProject")
        .Accepts<UpdateUserProjectDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userId:guid}/{projectId:guid}", async (Guid userId, Guid projectId, IUserProjectService userProjectService) =>
        {
            var result = await userProjectService.DeleteUserProjectAsync(userId, projectId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUserProject")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}