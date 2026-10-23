namespace SorteFederation.Portal.EndPoints;

public sealed class CountryEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(Country), "/countries", "Country");
}

