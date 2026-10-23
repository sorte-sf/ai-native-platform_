namespace SorteFederation.Portal.EndPoints;

public sealed class ApplicationDocsEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(ApplicationDoc), "/application-docs", "ApplicationDoc");
}

