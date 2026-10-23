using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class KnowledgeEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapKnowledgeEndpoints(endpoints);

public static IEndpointRouteBuilder MapKnowledgeEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/knowledge").WithTags("Knowledge").RequireAuthorization();
        group.MapPost("/documents", (CreateKnowledgeDocumentRequest request, KnowledgeService service, CancellationToken token) => service.AddAsync(request, token));
        group.MapPost("/search", (KnowledgeSearchRequest request, KnowledgeService service, CancellationToken token) => service.SearchAsync(request, token));
        group.MapPost("/search/vector", async (VectorKnowledgeSearchRequest request, KnowledgeService service, HttpContext http, CancellationToken token) =>
        {
            var claim = http.User.FindFirst("tenant_id")?.Value ?? http.User.FindFirst("company_id")?.Value;
            if (!http.User.IsInRole(BuilderRoleNames.SystemAdmin) &&
                (!Guid.TryParse(claim, out var claimedTenant) || claimedTenant != request.TenantId))
                return Results.Forbid();
            return Results.Ok(await service.VectorSearchAsync(request.TenantId, request.Embedding, request.Limit, token));
        });
        group.MapPost("/documents/{id:guid}/publish", async (Guid id, Guid tenantId, KnowledgeService service, CancellationToken token) => { await service.PublishAsync(tenantId, id, token); return Results.NoContent(); });
        group.MapPost("/ai/complete", (AiPromptRequest request, AiGatewayService service, CancellationToken token) => service.CompleteAsync(request, token))
            .RequireRateLimiting("builders-ai");
        return endpoints;
    }
}
