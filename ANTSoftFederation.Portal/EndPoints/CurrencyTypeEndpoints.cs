namespace SorteFederation.Portal.EndPoints;

public sealed class CurrencyTypeEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(CurrencyType), "/currency-types", "CurrencyType");
}

