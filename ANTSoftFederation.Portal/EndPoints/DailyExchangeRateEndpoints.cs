namespace SorteFederation.Portal.EndPoints;

public sealed class DailyExchangeRateEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(DailyExchangeRate), "/daily-exchange-rates", "DailyExchangeRate");
}

