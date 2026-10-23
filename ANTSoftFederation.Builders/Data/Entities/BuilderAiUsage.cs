namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderAiUsage : BuilderEntity
{
    public Guid? ApplicationId { get; set; }
    public string? UserId { get; set; }
    public string Feature { get; set; } = "";
    public string Provider { get; set; } = "";
    public string Model { get; set; } = "";
    public long InputTokens { get; set; }
    public long OutputTokens { get; set; }
    public bool IsEstimated { get; set; }
    public string Outcome { get; set; } = "completed";
    public string? FailureCode { get; set; }
    public long DurationMilliseconds { get; set; }
    public Guid RequestId { get; set; }
}
