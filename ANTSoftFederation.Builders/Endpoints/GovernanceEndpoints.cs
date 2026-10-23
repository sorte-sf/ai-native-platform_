using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;
using System.Security.Claims;
namespace SorteFederation.Builders.Endpoints;
public sealed class GovernanceEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapGovernanceEndpoints(endpoints);

public static IEndpointRouteBuilder MapGovernanceEndpoints(IEndpointRouteBuilder e)
    {
        e.MapPost("/application-versions/{versionId:guid}/approval", (Guid versionId, ApproveApplicationVersionRequest r, ClaimsPrincipal u, BuilderMetadataService s, CancellationToken t)=>s.ApproveVersion(r with {VersionId=versionId},u,t)).RequireAuthorization().WithTags("Governance");
        return e;
    }
}
