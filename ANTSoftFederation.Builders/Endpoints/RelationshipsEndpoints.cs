using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class RelationshipsEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapRelationshipsEndpoints(endpoints);

public static IEndpointRouteBuilder MapRelationshipsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/relationships", (CreateRelationshipRequest request, BuilderMetadataService service, CancellationToken token) => service.CreateRelationship(request, token)).RequireAuthorization().WithTags("Relationships");
        endpoints.MapGet("/relationships", (Guid tenantId, Guid applicationId, BuilderMetadataService service, CancellationToken token) => service.ListRelationships(tenantId, applicationId, token)).RequireAuthorization().WithTags("Relationships");
        return endpoints;
    }
}
