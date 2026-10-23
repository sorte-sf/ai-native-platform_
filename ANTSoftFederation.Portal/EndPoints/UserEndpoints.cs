namespace SorteFederation.Portal.EndPoints;

public sealed class UserEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(User), "/users", "User");
}

