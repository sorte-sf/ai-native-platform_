namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderKnowledgeDocument : BuilderEntity
{
    public Guid? ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public string? Source { get; set; }
    public string? MetadataJson { get; set; }
    public bool Published { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}
