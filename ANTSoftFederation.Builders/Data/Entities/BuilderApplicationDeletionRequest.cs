namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderApplicationDeletionRequest : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string RequestedByUserId { get; set; }
    public required string Reason { get; set; }
    public ApplicationDeletionRequestStatus Status { get; set; } = ApplicationDeletionRequestStatus.Pending;
    public string? ReviewedByUserId { get; set; }
    public string? ReviewReason { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}

public enum ApplicationDeletionRequestStatus : byte
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}
