using SorteFederation.Builders.Services;
using SorteFederation.Builders.Data;
using SorteFederation.Shared.EndPoints;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.Builders.Endpoints;

public sealed class SolutionDocumentsEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/applications/{applicationId:guid}/solutions", async (
            Guid applicationId, Guid tenantId, BuilderDbContext metadata, KnowledgeService knowledge,
            CancellationToken token) =>
        {
            if (!await metadata.Applications.AnyAsync(app => app.Id == applicationId && app.TenantId == tenantId, token))
                return Results.NotFound();

            var documents = await knowledge.ListApplicationDocumentsAsync(tenantId, applicationId, token);
            var results = documents.Select(document =>
            {
                string? fileId = null;
                string? fileName = document.Title;
                long? size = null;
                if (!string.IsNullOrWhiteSpace(document.MetadataJson))
                {
                    try
                    {
                        using var metadataJson = System.Text.Json.JsonDocument.Parse(document.MetadataJson);
                        var root = metadataJson.RootElement;
                        if (root.TryGetProperty("Id", out var id) || root.TryGetProperty("id", out id)) fileId = id.GetString();
                        if (root.TryGetProperty("Name", out var name) || root.TryGetProperty("name", out name)) fileName = name.GetString() ?? fileName;
                        if (root.TryGetProperty("Size", out var length) || root.TryGetProperty("size", out length)) size = length.GetInt64();
                    }
                    catch (System.Text.Json.JsonException) { }
                }
                return new { document.Id, document.ApplicationId, Title = document.Title, FileId = fileId,
                    FileName = fileName, Size = size, document.CreatedAt, document.Published };
            });
            return Results.Ok(results);
        })
            .RequireAuthorization()
            .WithName("ListApplicationSolutionDocuments").WithTags("Solutions");

        endpoints.MapPost("/applications/{applicationId:guid}/solutions", async (
            Guid applicationId, Guid tenantId, IFormFile file, SolutionDocumentService service,
            CancellationToken token) => Results.Ok(await service.UploadAsync(tenantId, applicationId, file, token)))
            .DisableAntiforgery()
            .RequireAuthorization(policy => policy.RequireRole(BuilderRoleNames.SystemAdmin,
                BuilderRoleNames.TenantAdmin, BuilderRoleNames.Developer))
            .WithName("UploadBuilderSolutionDocument").WithTags("Solutions");

        endpoints.MapPost("/applications/{applicationId:guid}/solutions/search", async (
            Guid applicationId, SolutionDocumentSearchRequest request, KnowledgeEmbeddingService embeddings,
            KnowledgeService knowledge, CancellationToken token) =>
        {
            if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length > 500)
                return Results.BadRequest("Search text is required and must not exceed 500 characters.");
            var vector = await embeddings.CreateEmbeddingAsync(request.Query, token);
            return Results.Ok(await knowledge.SearchApplicationAsync(request.TenantId, applicationId, vector, request.Limit, token));
        })
            .RequireAuthorization()
            .WithName("SearchApplicationSolutionDocuments").WithTags("Solutions");

        endpoints.MapPost("/knowledge/documents/{id:guid}/index", async (
            Guid id, Guid tenantId, KnowledgeEmbeddingService service, CancellationToken token) =>
            Results.Ok(new { ChunksIndexed = await service.IndexAsync(tenantId, id, token) }))
            .RequireAuthorization(policy => policy.RequireRole(BuilderRoleNames.SystemAdmin,
                BuilderRoleNames.TenantAdmin, BuilderRoleNames.Developer))
            .WithName("IndexBuilderKnowledgeDocument").WithTags("Solutions");
    }
}
