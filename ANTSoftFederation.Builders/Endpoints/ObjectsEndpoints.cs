using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class ObjectsEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapObjectsEndpoints(endpoints);

public static IEndpointRouteBuilder MapObjectsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/objects", (CreateObjectRequest request, BuilderMetadataService service, CancellationToken token) => service.CreateObject(request, token))
            .RequireAuthorization().WithName("CreateBuilderObject").WithTags("Objects");
        return endpoints;
    }
}
