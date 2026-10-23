namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderAuditEvent : BuilderEntity
{
    public required string EventType { get; set; }
    public required string ResourceType { get; set; }
    public Guid? ResourceId { get; set; }
    public string? ActorId { get; set; }
    public string? DataJson { get; set; }
    public string? CorrelationId { get; set; }
}
