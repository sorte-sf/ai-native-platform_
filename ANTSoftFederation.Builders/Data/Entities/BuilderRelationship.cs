namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderRelationship : BuilderEntity
{
    public Guid SourceObjectId { get; set; }
    public Guid TargetObjectId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Cardinality { get; set; }
}
