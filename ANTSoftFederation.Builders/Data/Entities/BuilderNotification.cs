namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderNotification : BuilderEntity
{
    public required string RecipientId { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public string? Type { get; set; }
    public string? ActionUrl { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
