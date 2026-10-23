namespace SorteFederation.Portal.EndPoints;

public sealed class UserPermissionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(UserPermission), "/user-permissions", "UserPermission");
}

