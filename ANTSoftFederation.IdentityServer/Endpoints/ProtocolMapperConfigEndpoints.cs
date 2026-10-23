
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class ProtocolMapperConfigEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/protocol-mapper-configs")
                      .WithTags("ProtocolMapperConfigs");

        group.MapGet("/", async (IProtocolMapperConfigService protocolMapperConfigService, [AsParameters] PaginationParams pagination) =>
        {
            var (protocolMapperConfigs, totalCount) = await protocolMapperConfigService.GetProtocolMapperConfigsAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = protocolMapperConfigs, TotalCount = totalCount });
        })
        .WithName("GetProtocolMapperConfigs")
        .Produces<ApiResponse<IEnumerable<ProtocolMapperConfigListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IProtocolMapperConfigService protocolMapperConfigService) =>
        {
            var protocolMapperConfig = await protocolMapperConfigService.GetProtocolMapperConfigByIdAsync(id);
            return protocolMapperConfig != null ? Results.Ok(protocolMapperConfig) : Results.NotFound();
        })
        .WithName("GetProtocolMapperConfigById")
        .Produces<ProtocolMapperConfigDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateProtocolMapperConfigDto createDto, IProtocolMapperConfigService protocolMapperConfigService) =>
        {
            var id = await protocolMapperConfigService.CreateProtocolMapperConfigAsync(createDto);
            return Results.Created($"/api/protocol-mapper-configs/{id}", new { Id = id });
        })
        .WithName("CreateProtocolMapperConfig")
        .Accepts<CreateProtocolMapperConfigDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProtocolMapperConfigDto updateDto, IProtocolMapperConfigService protocolMapperConfigService) =>
        {
            var result = await protocolMapperConfigService.UpdateProtocolMapperConfigAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateProtocolMapperConfig")
        .Accepts<UpdateProtocolMapperConfigDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IProtocolMapperConfigService protocolMapperConfigService) =>
        {
            var result = await protocolMapperConfigService.DeleteProtocolMapperConfigAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProtocolMapperConfig")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}