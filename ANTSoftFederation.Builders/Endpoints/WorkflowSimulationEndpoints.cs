using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SorteFederation.Builders.Data.Enums;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed record SimulateWorkflowRequest(Guid TenantId, string RecordDataJson,
    Dictionary<string, string>? ApprovalOutcomes = null);

public sealed class WorkflowSimulationEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/applications/{applicationId:guid}/workflows/{workflowId:guid}/simulate", async (
            Guid applicationId, Guid workflowId, SimulateWorkflowRequest request, ClaimsPrincipal user,
            BuilderDbContext db, CancellationToken token) =>
        {
            if (request.TenantId == Guid.Empty || string.IsNullOrWhiteSpace(request.RecordDataJson) ||
                request.RecordDataJson.Length > 1_000_000 || request.ApprovalOutcomes?.Count > 100)
                return Results.BadRequest(new { error = "Provide a tenant, sample JSON object up to 1 MB, and no more than 100 approval decisions." });

            var application = await db.Applications.AsNoTracking().SingleOrDefaultAsync(x =>
                x.Id == applicationId && x.TenantId == request.TenantId && x.RecordState == RecordState.Active, token);
            if (application is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, application)) return Results.Forbid();

            var workflow = await db.Workflows.AsNoTracking().SingleOrDefaultAsync(x =>
                x.Id == workflowId && x.ApplicationId == applicationId && x.TenantId == request.TenantId, token);
            if (workflow is null) return Results.NotFound();
            if (workflow.Status is not (DefinitionStatus.Draft or DefinitionStatus.Published))
                return Results.Conflict(new { error = "Only draft or published workflows can be simulated." });

            try
            {
                var graph = WorkflowGraphInterpreter.Parse(workflow.DefinitionJson);
                return Results.Ok(graph.Simulate(request.RecordDataJson, request.ApprovalOutcomes));
            }
            catch (JsonException)
            {
                return Results.UnprocessableEntity(new { error = "Sample data and workflow definition must contain valid JSON." });
            }
            catch (ArgumentException exception)
            {
                return Results.UnprocessableEntity(new { error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Results.UnprocessableEntity(new { error = exception.Message });
            }
        }).RequireAuthorization().WithTags("Workflow Runtime");
    }
}
