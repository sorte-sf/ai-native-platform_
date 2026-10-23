using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data;
using SorteFederation.Builders.Data.Domain;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace SorteFederation.Builders.Services;

public sealed class KnowledgeService(TenantRuntimeContextFactory factory, AuditService audit, IHttpContextAccessor accessor)
{
    public async Task<BuilderKnowledgeDocument> AddAsync(CreateKnowledgeDocumentRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Key and content are required.");
        if (request.Key.Trim().Length > 160 || request.Title.Trim().Length > 300 || request.Content.Length > 2_000_000 || request.Source?.Length > 500)
            throw new ArgumentException("Knowledge document exceeds the supported size limits.");
        var document = new BuilderKnowledgeDocument
        {
            TenantId = request.TenantId, Key = request.Key.Trim(), Title = request.Title.Trim(),
            Content = request.Content, Source = request.Source, MetadataJson = request.MetadataJson, ApplicationId = request.ApplicationId
        };
        await using var db = await factory.CreateAsync(request.TenantId, token);
        db.KnowledgeDocuments.Add(document);
        foreach (var (content, ordinal) in Chunk(document.Content, 1800).Select((content, ordinal) => (content, ordinal)))
        {
            db.KnowledgeChunks.Add(new BuilderKnowledgeChunk
            {
                TenantId = request.TenantId,
                DocumentId = document.Id,
                Ordinal = ordinal,
                Content = content,
                SourceUri = request.Source,
                IndexStatus = "pending"
            });
        }
        await db.SaveChangesAsync(token);
        await audit.Record(new CreateAuditEventRequest(request.TenantId, "knowledge.document.created", "knowledge-document", document.Id, null), accessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal(), accessor.HttpContext?.TraceIdentifier, token);
        return document;
    }

    private static IReadOnlyList<string> Chunk(string content, int size)
    {
        var chunks = new List<string>();
        for (var offset = 0; offset < content.Length; offset += size)
            chunks.Add(content.Substring(offset, Math.Min(size, content.Length - offset)));
        return chunks;
    }

    public async Task<List<BuilderKnowledgeDocument>> SearchAsync(KnowledgeSearchRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length > 500)
            throw new ArgumentException("Knowledge search text is required and must not exceed 500 characters.");
        var terms = request.Query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).Take(20).ToArray();
        await using var db = await factory.CreateAsync(request.TenantId, token);
        var query = db.KnowledgeDocuments.AsNoTracking().Where(x => x.TenantId == request.TenantId && x.Published);
        foreach (var term in terms)
        {
            var value = term;
            query = query.Where(x => EF.Functions.ILike(x.Title, $"%{value}%") || EF.Functions.ILike(x.Content, $"%{value}%"));
        }
        return await query.OrderByDescending(x => x.PublishedAt).Take(Math.Clamp(request.Limit, 1, 50)).ToListAsync(token);
    }

    public async Task<IReadOnlyList<BuilderKnowledgeChunk>> VectorSearchAsync(Guid authorizedTenantId, float[] embedding, int limit, CancellationToken token)
    {
        if (embedding.Length != 1536)
            throw new ArgumentException("A 1536-dimension embedding is required for vector search.");
        var vector = new Vector(embedding);
        await using var db = await factory.CreateAsync(authorizedTenantId, token);
        return await db.KnowledgeChunks.AsNoTracking()
            .Where(x => x.TenantId == authorizedTenantId && x.Embedding != null && x.IndexStatus == "indexed")
            .OrderBy(x => x.Embedding!.CosineDistance(vector))
            .Take(Math.Clamp(limit, 1, 50))
            .ToListAsync(token);
    }

    public async Task<IReadOnlyList<SolutionDocumentSearchResult>> SearchApplicationAsync(
        Guid tenantId, Guid applicationId, float[] embedding, int limit, CancellationToken token)
    {
        if (embedding.Length != 1536 || embedding.Any(value => !float.IsFinite(value)))
            throw new ArgumentException("A 1536-dimension finite embedding is required for document search.");
        var vector = new Vector(embedding);
        await using var db = await factory.CreateAsync(tenantId, token);
        var ranked = await (
            from chunk in db.KnowledgeChunks.AsNoTracking()
            join document in db.KnowledgeDocuments.AsNoTracking()
                on new { chunk.TenantId, Id = chunk.DocumentId } equals new { document.TenantId, Id = document.Id }
            where chunk.TenantId == tenantId && document.ApplicationId == applicationId && document.Published
                && chunk.Embedding != null && chunk.IndexStatus == "indexed"
            select new
            {
                chunk.DocumentId,
                document.Title,
                document.Source,
                chunk.Ordinal,
                chunk.Content,
                Distance = chunk.Embedding!.CosineDistance(vector)
            })
            .OrderBy(item => item.Distance)
            .Take(Math.Clamp(limit, 1, 20))
            .ToListAsync(token);
        return ranked.Select(item => new SolutionDocumentSearchResult(item.DocumentId, item.Title,
            item.Source, item.Ordinal, item.Content, Math.Max(0, 1 - item.Distance))).ToArray();
    }

    public async Task<IReadOnlyList<BuilderKnowledgeDocument>> ListApplicationDocumentsAsync(
        Guid tenantId, Guid applicationId, CancellationToken token)
    {
        await using var db = await factory.CreateAsync(tenantId, token);
        return await db.KnowledgeDocuments.AsNoTracking()
            .Where(document => document.TenantId == tenantId && document.ApplicationId == applicationId
                && document.Key.StartsWith($"solution:{applicationId:N}:"))
            .OrderByDescending(document => document.CreatedAt)
            .ToListAsync(token);
    }

    public async Task PublishAsync(Guid tenantId, Guid documentId, CancellationToken token)
    {
        await using var db = await factory.CreateAsync(tenantId, token);
        var document = await db.KnowledgeDocuments.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == documentId, token)
            ?? throw new KeyNotFoundException("Knowledge document was not found.");
        document.Published = true;
        document.PublishedAt = DateTimeOffset.UtcNow;
        document.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);
        await audit.Record(new CreateAuditEventRequest(tenantId, "knowledge.document.published", "knowledge-document", document.Id, null), accessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal(), accessor.HttpContext?.TraceIdentifier, token);
    }
}
