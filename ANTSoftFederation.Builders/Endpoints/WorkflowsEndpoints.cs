using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class WorkflowsEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapWorkflowsEndpoints(endpoints);

public static IEndpointRouteBuilder MapWorkflowsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/workflows", (Guid tenantId, Guid applicationId, BuilderMetadataService service, CancellationToken token) => service.ListWorkflows(tenantId, applicationId, token)).RequireAuthorization().WithTags("Workflows");
        endpoints.MapPost("/workflows", (CreateWorkflowRequest request, BuilderMetadataService service, CancellationToken token) => service.CreateWorkflow(request, token)).RequireAuthorization().WithTags("Workflows");
        endpoints.MapPut("/workflows/{workflowId:guid}", (Guid workflowId, UpdateWorkflowRequest request, BuilderMetadataService service, CancellationToken token) => service.UpdateWorkflow(request.TenantId, workflowId, request, token)).RequireAuthorization().WithTags("Workflows");
        endpoints.MapPost("/workflows/{workflowId:guid}/publish", (Guid workflowId, Guid tenantId, BuilderMetadataService service, CancellationToken token) => service.PublishWorkflow(tenantId, workflowId, token)).RequireAuthorization().WithTags("Workflows");
        endpoints.MapPost("/workflows/{workflowId:guid}/deactivate", (Guid workflowId, Guid tenantId, BuilderMetadataService service, CancellationToken token) => service.DeactivateWorkflow(tenantId, workflowId, token)).RequireAuthorization().WithTags("Workflows");
        endpoints.MapPost("/workflows/{workflowId:guid}/reactivate", (Guid workflowId, Guid tenantId, BuilderMetadataService service, CancellationToken token) => service.ReactivateWorkflow(tenantId, workflowId, token)).RequireAuthorization().WithTags("Workflows");
        return endpoints;
    }
}
