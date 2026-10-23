
namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderPlatformEvent : BuilderEntity
{
    public required string Type { get; set; }
    public required string PayloadJson { get; set; }
    public PlatformEventStatus Status { get; set; } = PlatformEventStatus.Pending;
    public int Attempts { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}
