using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;
namespace SorteFederation.Builders.Endpoints;
public sealed class FormRuntimeEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapFormRuntimeEndpoints(endpoints);

public static IEndpointRouteBuilder MapFormRuntimeEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/forms/validate", (ValidateFormRequest request, System.Security.Claims.ClaimsPrincipal user, FormRuntimeService service, CancellationToken token) => service.Validate(request, user, token)).RequireAuthorization().WithTags("Form Runtime");
        return endpoints;
    }
}
