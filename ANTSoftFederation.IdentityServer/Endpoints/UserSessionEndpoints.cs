
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class UserSessionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user-sessions")
                      .WithTags("UserSessions");

        group.MapGet("/", async (IUserSessionService userSessionService, [AsParameters] PaginationParams pagination) =>
        {
            var (userSessions, totalCount) = await userSessionService.GetUserSessionsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = userSessions, TotalCount = totalCount });
        })
        .WithName("GetUserSessions")
        .Produces<ApiResponse<IEnumerable<UserSessionListDto>>>();

        group.MapGet("/{userId:guid}/{projectId:guid}/{sessionId}", async (Guid userId, Guid projectId, string sessionId, IUserSessionService userSessionService) =>
        {
            var userSession = await userSessionService.GetUserSessionByIdAsync(userId, projectId, sessionId);
            return userSession != null ? Results.Ok(userSession) : Results.NotFound();
        })
        .WithName("GetUserSessionById")
        .Produces<UserSessionDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserSessionDto createDto, IUserSessionService userSessionService) =>
        {
            var result = await userSessionService.CreateUserSessionAsync(createDto);
            return result ? Results.Created($"/api/user-sessions/{createDto.UserId}/{createDto.ProjectId}/{createDto.SessionId}", new { UserId = createDto.UserId, ProjectId = createDto.ProjectId, SessionId = createDto.SessionId }) : Results.BadRequest("User session already exists");
        })
        .WithName("CreateUserSession")
        .Accepts<CreateUserSessionDto>("application/json")
        .Produces<bool>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{userId:guid}/{projectId:guid}/{sessionId}", async (Guid userId, Guid projectId, string sessionId, UpdateUserSessionDto updateDto, IUserSessionService userSessionService) =>
        {
            var result = await userSessionService.UpdateUserSessionAsync(userId, projectId, sessionId, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateUserSession")
        .Accepts<UpdateUserSessionDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userId:guid}/{projectId:guid}/{sessionId}", async (Guid userId, Guid projectId, string sessionId, IUserSessionService userSessionService) =>
        {
            var result = await userSessionService.DeleteUserSessionAsync(userId, projectId, sessionId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUserSession")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}