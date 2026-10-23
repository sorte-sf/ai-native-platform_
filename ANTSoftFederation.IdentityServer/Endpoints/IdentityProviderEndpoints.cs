
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class IdentityProviderEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/identity-providers")
                      .WithTags("IdentityProviders");

        group.MapGet("/", async (IIdentityProviderService identityProviderService, [AsParameters] PaginationParams pagination) =>
        {
            var (identityProviders, totalCount) = await identityProviderService.GetIdentityProvidersAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = identityProviders, TotalCount = totalCount });
        })
        .WithName("GetIdentityProviders")
        .Produces<ApiResponse<IEnumerable<IdentityProviderListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IIdentityProviderService identityProviderService) =>
        {
            var identityProvider = await identityProviderService.GetIdentityProviderByIdAsync(id);
            return identityProvider != null ? Results.Ok(identityProvider) : Results.NotFound();
        })
        .WithName("GetIdentityProviderById")
        .Produces<IdentityProviderDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateIdentityProviderDto createDto, IIdentityProviderService identityProviderService) =>
        {
            var id = await identityProviderService.CreateIdentityProviderAsync(createDto);
            return Results.Created($"/api/identity-providers/{id}", new { Id = id });
        })
        .WithName("CreateIdentityProvider")
        .Accepts<CreateIdentityProviderDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateIdentityProviderDto updateDto, IIdentityProviderService identityProviderService) =>
        {
            var result = await identityProviderService.UpdateIdentityProviderAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateIdentityProvider")
        .Accepts<UpdateIdentityProviderDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IIdentityProviderService identityProviderService) =>
        {
            var result = await identityProviderService.DeleteIdentityProviderAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteIdentityProvider")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}