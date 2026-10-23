using Pgvector;

namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderKnowledgeChunk : BuilderEntity
{
    public Guid DocumentId { get; set; }
    public int Ordinal { get; set; }
    public required string Content { get; set; }
    public string? SourceUri { get; set; }
    public string? EmbeddingModel { get; set; }
    public int? EmbeddingDimensions { get; set; }
    public Vector? Embedding { get; set; }
    public string IndexStatus { get; set; } = "pending";
    public DateTimeOffset? IndexedAt { get; set; }
}
