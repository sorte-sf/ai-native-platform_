
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class UserRoleEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user-roles")
                      .WithTags("UserRoles");

        group.MapGet("/", async (IUserRoleService userRoleService, [AsParameters] PaginationParams pagination) =>
        {
            var (userRoles, totalCount) = await userRoleService.GetUserRolesAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = userRoles, TotalCount = totalCount });
        })
        .WithName("GetUserRoles")
        .Produces<ApiResponse<IEnumerable<UserRoleListDto>>>();

        group.MapGet("/{userId:guid}/{roleId:guid}", async (Guid userId, Guid roleId, IUserRoleService userRoleService) =>
        {
            var userRole = await userRoleService.GetUserRoleByIdAsync(userId, roleId);
            return userRole != null ? Results.Ok(userRole) : Results.NotFound();
        })
        .WithName("GetUserRoleById")
        .Produces<UserRoleDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserRoleDto createDto, IUserRoleService userRoleService) =>
        {
            var result = await userRoleService.CreateUserRoleAsync(createDto);
            return result ? Results.Created($"/api/user-roles/{createDto.UserId}/{createDto.RoleId}", new { UserId = createDto.UserId, RoleId = createDto.RoleId }) : Results.BadRequest("User role already exists");
        })
        .WithName("CreateUserRole")
        .Accepts<CreateUserRoleDto>("application/json")
        .Produces<bool>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{userId:guid}/{roleId:guid}", async (Guid userId, Guid roleId, UpdateUserRoleDto updateDto, IUserRoleService userRoleService) =>
        {
            var result = await userRoleService.UpdateUserRoleAsync(userId, roleId, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateUserRole")
        .Accepts<UpdateUserRoleDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userId:guid}/{roleId:guid}", async (Guid userId, Guid roleId, IUserRoleService userRoleService) =>
        {
            var result = await userRoleService.DeleteUserRoleAsync(userId, roleId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUserRole")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}