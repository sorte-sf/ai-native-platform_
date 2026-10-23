namespace SorteFederation.Portal.EndPoints;

public sealed class GroupPermissionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(GroupPermission), "/group-permissions", "GroupPermission");
}

