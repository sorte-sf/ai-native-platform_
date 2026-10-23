using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class WorkflowRuntimeEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapWorkflowRuntimeEndpoints(endpoints);

public static IEndpointRouteBuilder MapWorkflowRuntimeEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/workflow-instances", async (StartWorkflowRequest request, TenantWorkflowRuntimeService service, PurchaseApprovalRuntimeService approvals, CancellationToken token) =>
        {
            if (await approvals.IsApprovalWorkflow(request.TenantId, request.WorkflowId, token)) return Results.BadRequest("Submit purchase requests through the approval submission action.");
            return Results.Ok(await service.Start(request, token));
        }).RequireAuthorization().WithTags("Workflow Runtime");
        endpoints.MapPost("/workflow-tasks/{taskId:guid}/complete", async (Guid taskId, CompleteWorkflowTaskRequest request, TenantWorkflowRuntimeService service, PurchaseApprovalRuntimeService approvals, CancellationToken token) =>
        {
            if (await approvals.IsApprovalTask(request.TenantId, taskId, token)) return Results.BadRequest("Purchase approval tasks require an approve or reject decision.");
            return await service.Complete(request with { TaskId = taskId }, token) ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization().WithTags("Workflow Runtime");
        endpoints.MapPost("/workflow-instances/{instanceId:guid}/transition", (Guid instanceId, TransitionWorkflowRequest request, TenantWorkflowRuntimeService service, CancellationToken token) => service.Transition(request with { InstanceId = instanceId }, token)).RequireAuthorization().WithTags("Workflow Runtime");
        endpoints.MapGet("/workflow-instances", (Guid tenantId, TenantWorkflowRuntimeService service, CancellationToken token) => service.List(tenantId, token)).RequireAuthorization().WithTags("Workflow Runtime");
        endpoints.MapGet("/workflow-tasks/inbox", (Guid tenantId, TenantWorkflowRuntimeService service, CancellationToken token) => service.Inbox(tenantId, token)).RequireAuthorization().WithTags("Workflow Runtime");
        endpoints.MapPost("/workflow-instances/{instanceId:guid}/cancel", async (Guid instanceId, Guid tenantId, TenantWorkflowRuntimeService service, CancellationToken token) => await service.Cancel(tenantId, instanceId, token) ? Results.NoContent() : Results.NotFound()).RequireAuthorization().WithTags("Workflow Runtime");
        endpoints.MapPut("/workflow-tasks/{taskId:guid}/assignment", async (Guid taskId, AssignWorkflowTaskRequest request, TenantWorkflowRuntimeService service, CancellationToken token) => await service.Assign(request.TenantId, taskId, request.AssignedTo, token) ? Results.NoContent() : Results.NotFound()).RequireAuthorization().WithTags("Workflow Runtime");
        endpoints.MapPost("/purchase-approvals/submit", (SubmitPurchaseApprovalRequest request, PurchaseApprovalRuntimeService service, CancellationToken token) => service.Submit(request, token)).RequireAuthorization().WithTags("Purchase Approvals");
        endpoints.MapPost("/purchase-approvals/tasks/{taskId:guid}/decision", async (Guid taskId, PurchaseApprovalDecisionRequest request, PurchaseApprovalRuntimeService service, CancellationToken token) => await service.Decide(request.TenantId, taskId, request, token) ? Results.NoContent() : Results.NotFound()).RequireAuthorization().WithTags("Purchase Approvals");
        return endpoints;
    }
}
