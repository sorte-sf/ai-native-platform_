namespace SorteFederation.Portal.EndPoints;

public sealed class RoleEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(Role), "/roles", "Role");
}

