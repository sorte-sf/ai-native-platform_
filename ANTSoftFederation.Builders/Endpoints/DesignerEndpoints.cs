using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;
namespace SorteFederation.Builders.Endpoints;
public sealed class DesignerEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapDesignerEndpoints(endpoints);

public static IEndpointRouteBuilder MapDesignerEndpoints(IEndpointRouteBuilder e)
    {
        e.MapGet("/designer/applications/{applicationId:guid}", async (Guid applicationId, Guid tenantId, FormDesignerService service, CancellationToken token) =>
            await service.GetApplication(tenantId, applicationId, token) is { } app ? Results.Ok(app) : Results.NotFound())
            .RequireAuthorization().WithName("GetBuilderDesignerApplication").WithTags("Designer");
        e.MapPut("/designer/draft", async (SaveDesignerDraftRequest request, FormDesignerService service, CancellationToken token) =>
        {
            try { return Results.Ok(await service.SaveDraft(request, token)); }
            catch (FormBlockNotFoundException ex) { return Results.Conflict(new { message = ex.Message }); }
        })
            .RequireAuthorization().WithName("SaveBuilderDesignerDraft").WithTags("Designer");
        e.MapPost("/workflow-transitions", (CreateWorkflowTransitionRequest r, PlatformRuntimeService s, CancellationToken t)=>s.AddTransition(r,t)).RequireAuthorization().WithTags("Designer");
        e.MapPost("/reports", (CreateReportRequest r, PlatformRuntimeService s, CancellationToken t)=>s.AddReport(r,t)).RequireAuthorization().WithTags("Designer");
        e.MapPost("/dashboards", (CreateDashboardRequest r, PlatformRuntimeService s, CancellationToken t)=>s.AddDashboard(r,t)).RequireAuthorization().WithTags("Designer");
        e.MapPost("/dashboard-widgets", (CreateDashboardWidgetRequest r, PlatformRuntimeService s, CancellationToken t)=>s.AddWidget(r,t)).RequireAuthorization().WithTags("Designer");
        return e;
    }
}
