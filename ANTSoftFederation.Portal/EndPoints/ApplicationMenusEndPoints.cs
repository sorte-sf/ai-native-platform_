namespace SorteFederation.Portal.EndPoints;

public sealed class ApplicationMenusEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(ApplicationMenu), "/application-menus", "ApplicationMenu");
}

