using Microsoft.AspNetCore.Routing;

namespace SorteFederation.Shared.EndPoints;

public interface IAppEndPoints
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
