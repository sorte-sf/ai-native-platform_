using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;
using System.Security.Claims;

namespace SorteFederation.Builders.Endpoints;

public sealed class RulesEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapRulesEndpoints(endpoints);

public static IEndpointRouteBuilder MapRulesEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/rules/evaluate", (EvaluateRulesRequest request, ClaimsPrincipal user, RulesRuntimeService service, CancellationToken token) => service.EvaluateAsync(request, user, token))
            .WithTags("Rules");
        return endpoints;
    }
}
