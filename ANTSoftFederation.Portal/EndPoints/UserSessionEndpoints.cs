namespace SorteFederation.Portal.EndPoints;

public sealed class UserSessionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(UserSession), "/user-sessions", "UserSession");
}

