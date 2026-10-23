namespace SorteFederation.Portal.EndPoints;

public sealed class GroupEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(Group), "/groups", "Group");
}

