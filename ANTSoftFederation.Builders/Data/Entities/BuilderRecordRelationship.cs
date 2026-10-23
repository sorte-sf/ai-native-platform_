namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderRecordRelationship : BuilderEntity
{
    public Guid RelationshipId { get; set; }
    public Guid SourceRecordId { get; set; }
    public Guid TargetRecordId { get; set; }
}
