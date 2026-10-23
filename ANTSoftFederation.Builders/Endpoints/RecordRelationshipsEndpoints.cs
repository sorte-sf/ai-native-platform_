using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class RecordRelationshipsEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapRecordRelationshipsEndpoints(endpoints);

public static IEndpointRouteBuilder MapRecordRelationshipsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/record-relationships", (CreateRecordRelationshipRequest request, TenantRelationshipService service, CancellationToken token) => service.Create(request, token)).RequireAuthorization().WithTags("Record Relationships");
        endpoints.MapGet("/record-relationships", (Guid tenantId, Guid relationshipId, Guid? sourceRecordId, TenantRelationshipService service, CancellationToken token) => service.List(tenantId, relationshipId, sourceRecordId, token)).RequireAuthorization().WithTags("Record Relationships");
        endpoints.MapDelete("/record-relationships/{id:guid}", (Guid id, Guid tenantId, TenantRelationshipService service, CancellationToken token) => service.Delete(tenantId, id, token)).RequireAuthorization().WithTags("Record Relationships");
        return endpoints;
    }
}
