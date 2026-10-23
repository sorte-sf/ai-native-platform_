namespace SorteFederation.Portal.EndPoints;

public sealed class RolePermissionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(RolePermission), "/role-permissions", "RolePermission");
}

