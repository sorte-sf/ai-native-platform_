namespace SorteFederation.Portal.EndPoints;

public sealed class ApplicationModulesEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(ApplicationModule), "/application-modules", "ApplicationModule");
}

