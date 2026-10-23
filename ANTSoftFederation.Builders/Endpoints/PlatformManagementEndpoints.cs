using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;
namespace SorteFederation.Builders.Endpoints;
public sealed class PlatformManagementEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapPlatformManagementEndpoints(endpoints);

public static IEndpointRouteBuilder MapPlatformManagementEndpoints(IEndpointRouteBuilder e)
    {
        e.MapPost("/feature-flags", (CreateFeatureFlagRequest r, PlatformRuntimeService s, CancellationToken t)=>s.AddFeatureFlag(r,t)).RequireAuthorization().WithTags("Platform Management");
        e.MapGet("/feature-flags/{applicationId:guid}/{key}", (Guid applicationId, string key, Guid tenantId, FeatureFlagService s, ClaimsPrincipal user, CancellationToken t) => s.IsEnabledAsync(tenantId, applicationId, key, user, t)).RequireAuthorization().WithTags("Platform Management");
        return e;
    }
}
