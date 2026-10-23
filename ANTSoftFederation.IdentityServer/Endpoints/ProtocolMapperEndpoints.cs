
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class ProtocolMapperEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/protocol-mappers")
                      .WithTags("ProtocolMappers");

        group.MapGet("/", async (IProtocolMapperService protocolMapperService, [AsParameters] PaginationParams pagination) =>
        {
            var (protocolMappers, totalCount) = await protocolMapperService.GetProtocolMappersAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = protocolMappers, TotalCount = totalCount });
        })
        .WithName("GetProtocolMappers")
        .Produces<ApiResponse<IEnumerable<ProtocolMapperListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IProtocolMapperService protocolMapperService) =>
        {
            var protocolMapper = await protocolMapperService.GetProtocolMapperByIdAsync(id);
            return protocolMapper != null ? Results.Ok(protocolMapper) : Results.NotFound();
        })
        .WithName("GetProtocolMapperById")
        .Produces<ProtocolMapperDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateProtocolMapperDto createDto, IProtocolMapperService protocolMapperService) =>
        {
            var id = await protocolMapperService.CreateProtocolMapperAsync(createDto);
            return Results.Created($"/api/protocol-mappers/{id}", new { Id = id });
        })
        .WithName("CreateProtocolMapper")
        .Accepts<CreateProtocolMapperDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProtocolMapperDto updateDto, IProtocolMapperService protocolMapperService) =>
        {
            var result = await protocolMapperService.UpdateProtocolMapperAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateProtocolMapper")
        .Accepts<UpdateProtocolMapperDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IProtocolMapperService protocolMapperService) =>
        {
            var result = await protocolMapperService.DeleteProtocolMapperAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProtocolMapper")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}