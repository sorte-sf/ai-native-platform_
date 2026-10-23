namespace SorteFederation.Portal.EndPoints;

public sealed class ApplicationVersioningEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(ApplicationVersioning), "/application-versioning", "ApplicationVersioning");
}

