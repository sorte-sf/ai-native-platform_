namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderScheduledJob : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Schedule { get; set; }
    public string ScheduleType { get; set; } = "recurring";
    public DateTimeOffset? RunAt { get; set; }
    public string TimeZoneId { get; set; } = "Europe/Istanbul";
    public Guid? NextTickerId { get; set; }
    public required string ActionType { get; set; }
    public string ActionJson { get; set; } = "{}";
    public bool Enabled { get; set; } = true;
    public DateTimeOffset? LastRunAt { get; set; }
    public DateTimeOffset? NextRunAt { get; set; }
}
