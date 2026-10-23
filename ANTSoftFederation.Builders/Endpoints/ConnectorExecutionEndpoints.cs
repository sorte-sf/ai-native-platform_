
using Microsoft.AspNetCore.Mvc;

namespace SorteFederation.Builders.Endpoints;

public sealed class ConnectorExecutionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapConnectorExecutionEndpoints(endpoints);

public static IEndpointRouteBuilder MapConnectorExecutionEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/connectors/{connectorId:guid}/execute", (Guid connectorId, ExecuteConnectorRequest request, [FromServices] ConnectorExecutionService service, CancellationToken token) => service.Execute(request with { ConnectorId = connectorId }, token)).RequireAuthorization().WithTags("Connector Runtime");
        return endpoints;
    }
}
