
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class RoleEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/roles")
                      .WithTags("Roles");

        group.MapGet("/", async (IRoleService roleService, [AsParameters] PaginationParams pagination) =>
        {
            var (roles, totalCount) = await roleService.GetRolesAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = roles, TotalCount = totalCount });
        })
        .WithName("GetRoles")
        .Produces<ApiResponse<IEnumerable<RoleListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IRoleService roleService) =>
        {
            var role = await roleService.GetRoleByIdAsync(id);
            return role != null ? Results.Ok(role) : Results.NotFound();
        })
        .WithName("GetRoleById")
        .Produces<RoleDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateRoleDto createDto, IRoleService roleService) =>
        {
            var id = await roleService.CreateRoleAsync(createDto);
            return Results.Created($"/api/roles/{id}", new { Id = id });
        })
        .WithName("CreateRole")
        .Accepts<CreateRoleDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateRoleDto updateDto, IRoleService roleService) =>
        {
            var result = await roleService.UpdateRoleAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateRole")
        .Accepts<UpdateRoleDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IRoleService roleService) =>
        {
            var result = await roleService.DeleteRoleAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteRole")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}