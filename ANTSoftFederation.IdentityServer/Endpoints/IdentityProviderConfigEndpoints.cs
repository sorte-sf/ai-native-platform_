
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class IdentityProviderConfigEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/identity-provider-configs")
                      .WithTags("IdentityProviderConfigs");

        group.MapGet("/", async (IIdentityProviderConfigService identityProviderConfigService, [AsParameters] PaginationParams pagination) =>
        {
            var (identityProviderConfigs, totalCount) = await identityProviderConfigService.GetIdentityProviderConfigsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = identityProviderConfigs, TotalCount = totalCount });
        })
        .WithName("GetIdentityProviderConfigs")
        .Produces<ApiResponse<IEnumerable<IdentityProviderConfigListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IIdentityProviderConfigService identityProviderConfigService) =>
        {
            var identityProviderConfig = await identityProviderConfigService.GetIdentityProviderConfigByIdAsync(id);
            return identityProviderConfig != null ? Results.Ok(identityProviderConfig) : Results.NotFound();
        })
        .WithName("GetIdentityProviderConfigById")
        .Produces<IdentityProviderConfigDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateIdentityProviderConfigDto createDto, IIdentityProviderConfigService identityProviderConfigService) =>
        {
            var id = await identityProviderConfigService.CreateIdentityProviderConfigAsync(createDto);
            return Results.Created($"/api/identity-provider-configs/{id}", new { Id = id });
        })
        .WithName("CreateIdentityProviderConfig")
        .Accepts<CreateIdentityProviderConfigDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateIdentityProviderConfigDto updateDto, IIdentityProviderConfigService identityProviderConfigService) =>
        {
            var result = await identityProviderConfigService.UpdateIdentityProviderConfigAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateIdentityProviderConfig")
        .Accepts<UpdateIdentityProviderConfigDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IIdentityProviderConfigService identityProviderConfigService) =>
        {
            var result = await identityProviderConfigService.DeleteIdentityProviderConfigAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteIdentityProviderConfig")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}