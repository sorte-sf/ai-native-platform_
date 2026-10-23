using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class RecordsEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapRecordsEndpoints(endpoints);

public static IEndpointRouteBuilder MapRecordsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/objects/{objectId:guid}/record-capabilities", (Guid objectId, Guid tenantId, HttpContext context, BuilderAuthorizationService authorization, CancellationToken token) =>
            authorization.ObjectCapabilities(tenantId, objectId, context.User, token)).RequireAuthorization().WithTags("Records");
        endpoints.MapPost("/records", (CreateRecordRequest request, TenantRecordService service, CancellationToken token) => service.Create(request, token))
            .WithName("CreateBuilderRecord").WithTags("Records");

        endpoints.MapGet("/objects/{objectId:guid}/records", (Guid objectId, Guid tenantId, string? search, TenantRecordService service, CancellationToken token) => service.Query(tenantId, objectId, search, token))
            .WithName("QueryBuilderRecords").WithTags("Records");

        endpoints.MapGet("/records/{id:guid}", async (Guid id, Guid tenantId, TenantRecordService service, CancellationToken token) =>
            await service.Get(tenantId, id, token) is { } record ? Results.Ok(record) : Results.NotFound()).WithTags("Records");

        endpoints.MapPut("/records/{id:guid}", async (Guid id, UpdateRecordRequest request, TenantRecordService service, CancellationToken token) =>
            Results.Ok(await service.Update(request.TenantId, id, request, token))).WithTags("Records");

        endpoints.MapDelete("/records/{id:guid}", async (Guid id, Guid tenantId, TenantRecordService service, CancellationToken token) =>
            await service.Delete(tenantId, id, token) ? Results.NoContent() : Results.NotFound()).WithTags("Records");
        return endpoints;
    }
}
