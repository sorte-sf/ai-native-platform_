 
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class ClientScopeEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/client-scopes")
                      .WithTags("ClientScopes");

        group.MapGet("/", async (IClientScopeService clientScopeService, [AsParameters] PaginationParams pagination) =>
        {
            var (clientScopes, totalCount) = await clientScopeService.GetClientScopesAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = clientScopes, TotalCount = totalCount });
        })
        .WithName("GetClientScopes")
        .Produces<ApiResponse<IEnumerable<ClientScopeListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IClientScopeService clientScopeService) =>
        {
            var clientScope = await clientScopeService.GetClientScopeByIdAsync(id);
            return clientScope != null ? Results.Ok(clientScope) : Results.NotFound();
        })
        .WithName("GetClientScopeById")
        .Produces<ClientScopeDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClientScopeDto createDto, IClientScopeService clientScopeService) =>
        {
            var id = await clientScopeService.CreateClientScopeAsync(createDto);
            return Results.Created($"/api/client-scopes/{id}", new { Id = id });
        })
        .WithName("CreateClientScope")
        .Accepts<CreateClientScopeDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateClientScopeDto updateDto, IClientScopeService clientScopeService) =>
        {
            var result = await clientScopeService.UpdateClientScopeAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateClientScope")
        .Accepts<UpdateClientScopeDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IClientScopeService clientScopeService) =>
        {
            var result = await clientScopeService.DeleteClientScopeAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteClientScope")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}