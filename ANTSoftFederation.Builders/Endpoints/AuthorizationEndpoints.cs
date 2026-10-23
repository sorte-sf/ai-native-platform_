 
namespace SorteFederation.Builders.Endpoints;

public sealed class AuthorizationEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapAuthorizationEndpoints(endpoints);

public static IEndpointRouteBuilder MapAuthorizationEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/applications/{applicationId:guid}/permissions/check", async (Guid applicationId, Guid tenantId, string resourceType, string action, ClaimsPrincipal user, BuilderMetadataService service, CancellationToken token) =>
        {
            var allowed = await service.HasPermission(tenantId, applicationId, resourceType, action, user, token);
            return allowed ? Results.Ok(new { allowed = true }) : Results.Forbid();
        }).RequireAuthorization().WithTags("Authorization");
        return endpoints;
    }
}
