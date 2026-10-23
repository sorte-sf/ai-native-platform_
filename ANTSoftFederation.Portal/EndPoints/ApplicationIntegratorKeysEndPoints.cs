namespace SorteFederation.Portal.EndPoints;

public sealed class ApplicationIntegratorKeysEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(ApplicationIntegratorKey), "/application-integrator-keys", "ApplicationIntegratorKey");
}

