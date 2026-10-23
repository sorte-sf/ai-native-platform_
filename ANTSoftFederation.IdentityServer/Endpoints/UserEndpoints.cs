
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class UserEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/", async (IUserService userService, [AsParameters] PaginationParams pagination) =>
        {
            var (users, totalCount) = await userService.GetUsersAsync(pagination.Page, pagination.PageSize, pagination.Search);
            
            return Results.Ok(new { Data = users, TotalCount = totalCount });
        })
        .WithName("GetUsers")
        .Produces<ApiResponse<IEnumerable<UserListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IUserService userService) =>
        {
            var user = await userService.GetUserByIdAsync(id);
            return user != null ? Results.Ok(user) : Results.NotFound();
        })
        .WithName("GetUserById")
        .Produces<UserDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserDto createDto, IUserService userService) =>
        {
            var id = await userService.CreateUserAsync(createDto);
            return Results.Created($"/api/users/{id}", new { Id = id });
        })
        .WithName("CreateUser")
        .Accepts<CreateUserDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserDto updateDto, IUserService userService) =>
        {
            var result = await userService.UpdateUserAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateUser")
        .Accepts<UpdateUserDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IUserService userService) =>
        {
            var result = await userService.DeleteUserAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUser")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}