using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class FieldsEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapFieldsEndpoints(endpoints);

public static IEndpointRouteBuilder MapFieldsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/fields", (CreateFieldRequest request, BuilderMetadataService service, CancellationToken token) => service.CreateField(request, token))
            .RequireAuthorization().WithName("CreateBuilderField").WithTags("Fields");
        return endpoints;
    }
}
