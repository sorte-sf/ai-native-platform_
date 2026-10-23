 
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class ClientRoleEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/client-roles")
                      .WithTags("ClientRoles");

        group.MapGet("/", async (IClientRoleService clientRoleService, [AsParameters] PaginationParams pagination) =>
        {
            var (clientRoles, totalCount) = await clientRoleService.GetClientRolesAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = clientRoles, TotalCount = totalCount });
        })
        .WithName("GetClientRoles")
        .Produces<ApiResponse<IEnumerable<ClientRoleListDto>>>();

        group.MapGet("/{clientId:guid}/{roleId:guid}", async (Guid clientId, Guid roleId, IClientRoleService clientRoleService) =>
        {
            var clientRole = await clientRoleService.GetClientRoleByIdAsync(clientId, roleId);
            return clientRole != null ? Results.Ok(clientRole) : Results.NotFound();
        })
        .WithName("GetClientRoleById")
        .Produces<ClientRoleDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClientRoleDto createDto, IClientRoleService clientRoleService) =>
        {
            var result = await clientRoleService.CreateClientRoleAsync(createDto);
            return result ? Results.Created($"/api/client-roles/{createDto.ClientId}/{createDto.RoleId}", new { ClientId = createDto.ClientId, RoleId = createDto.RoleId }) : Results.BadRequest("Client role already exists");
        })
        .WithName("CreateClientRole")
        .Accepts<CreateClientRoleDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{clientId:guid}/{roleId:guid}", async (Guid clientId, Guid roleId, UpdateClientRoleDto updateDto, IClientRoleService clientRoleService) =>
        {
            var result = await clientRoleService.UpdateClientRoleAsync(clientId, roleId, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateClientRole")
        .Accepts<UpdateClientRoleDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{clientId:guid}/{roleId:guid}", async (Guid clientId, Guid roleId, IClientRoleService clientRoleService) =>
        {
            var result = await clientRoleService.DeleteClientRoleAsync(clientId, roleId);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteClientRole")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}