namespace SorteFederation.Portal.EndPoints;

public sealed class PermissionEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(Permission), "/permissions", "Permission");
}

