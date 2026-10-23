namespace SorteFederation.Portal.EndPoints;

public sealed class UserApplicationEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(UserApplication), "/user-applications", "UserApplication");
}

