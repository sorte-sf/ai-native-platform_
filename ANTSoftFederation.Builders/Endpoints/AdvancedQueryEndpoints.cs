
namespace SorteFederation.Builders.Endpoints;

public sealed class AdvancedQueryEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapAdvancedQueryEndpoints(endpoints);
        public static IEndpointRouteBuilder MapAdvancedQueryEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/records/query", (DynamicQueryRequest request, DynamicQueryService service, CancellationToken token) => service.Query(request, token)).RequireAuthorization().WithTags("Reporting");
        return endpoints;
    }
}
