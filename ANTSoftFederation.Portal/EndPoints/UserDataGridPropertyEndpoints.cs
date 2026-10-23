namespace SorteFederation.Portal.EndPoints;

public sealed class UserDataGridPropertyEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(UserDataGridProperty), "/user-data-grid-properties", "UserDataGridProperty");
}

