namespace SorteFederation.Portal.EndPoints;

public sealed class CompanyApplicationEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(CompanyApplication), "/company-applications", "CompanyApplication");
}

