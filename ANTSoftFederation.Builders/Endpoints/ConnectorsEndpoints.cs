 
namespace SorteFederation.Builders.Endpoints;

public sealed class ConnectorsEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapConnectorsEndpoints(endpoints);

public static IEndpointRouteBuilder MapConnectorsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/connectors", (CreateConnectorRequest request, BuilderMetadataService service, CancellationToken token) => service.CreateConnector(request, token)).RequireAuthorization().WithTags("Connectors");
        return endpoints;
    }
}
