namespace SorteFederation.Portal.EndPoints;

public sealed class UserRoleEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(UserRole), "/user-roles", "UserRole");
}

