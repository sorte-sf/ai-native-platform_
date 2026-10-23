
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class ClientEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/clients")
                      .WithTags("Clients");

        group.MapGet("/", async (IClientService clientService, [AsParameters] PaginationParams pagination) =>
        {
            var (clients, totalCount) = await clientService.GetClientsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = clients, TotalCount = totalCount });
        })
        .WithName("GetClients")
        .Produces<ApiResponse<IEnumerable<ClientListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IClientService clientService) =>
        {
            var client = await clientService.GetClientByIdAsync(id);
            return client != null ? Results.Ok(client) : Results.NotFound();
        })
        .WithName("GetClientById")
        .Produces<ClientDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClientDto createDto, IClientService clientService) =>
        {
            var id = await clientService.CreateClientAsync(createDto);
            return Results.Created($"/api/clients/{id}", new { Id = id });
        })
        .WithName("CreateClient")
        .Accepts<CreateClientDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateClientDto updateDto, IClientService clientService) =>
        {
            var result = await clientService.UpdateClientAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateClient")
        .Accepts<UpdateClientDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IClientService clientService) =>
        {
            var result = await clientService.DeleteClientAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteClient")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}