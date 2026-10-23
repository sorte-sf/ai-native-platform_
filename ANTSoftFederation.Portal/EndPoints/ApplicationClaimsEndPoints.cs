namespace SorteFederation.Portal.EndPoints;

public sealed class ApplicationClaimsEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(ApplicationClaim), "/application-claims", "ApplicationClaim");
}

