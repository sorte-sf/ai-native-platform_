namespace SorteFederation.Portal.EndPoints;

public sealed class GroupRoleEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(GroupRole), "/group-roles", "GroupRole");
}

