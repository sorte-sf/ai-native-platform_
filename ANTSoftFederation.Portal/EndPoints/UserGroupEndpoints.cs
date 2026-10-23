namespace SorteFederation.Portal.EndPoints;

public sealed class UserGroupEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(UserGroup), "/user-groups", "UserGroup");
}

