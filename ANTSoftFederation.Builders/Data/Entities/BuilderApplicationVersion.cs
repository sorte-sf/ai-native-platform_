using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderApplicationVersion : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public string VersionNumber { get; set; }
    public GovernanceState State { get; set; } = GovernanceState.Draft;
    public required string SnapshotJson { get; set; }
    public string? ValidationErrorsJson { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}
