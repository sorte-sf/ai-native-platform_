
namespace SorteFederation.Builders.Endpoints;

public sealed class ConnectorCredentialsEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapConnectorCredentialsEndpoints(endpoints);

public static IEndpointRouteBuilder MapConnectorCredentialsEndpoints(IEndpointRouteBuilder e)
    {
        e.MapPost("/connector-credentials", (CreateCredentialRequest r, PlatformRuntimeService s, CancellationToken t)=>s.AddCredential(r,t)).RequireAuthorization().WithTags("Connector Credentials");

        e.MapPost("/connector-credentials/{credentialId:guid}/revoke", async (Guid credentialId, Guid tenantId, PlatformRuntimeService s, CancellationToken t)=>await s.RevokeCredential(tenantId,credentialId,t)?Results.NoContent():Results.NotFound()).RequireAuthorization().WithTags("Connector Credentials");

        return e;
    }
}
