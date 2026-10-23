namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderWorkflowTask : BuilderEntity
{
    public Guid WorkflowInstanceId { get; set; }
    public BuilderWorkflowInstance WorkflowInstance { get; set; } = null!;
    public required string Name { get; set; }
    public string? AssignedTo { get; set; }
    public bool Completed { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? EscalateAt { get; set; }
    public string? EscalateTo { get; set; }
    public string? BranchKey { get; set; }
    public string? DependsOn { get; set; }
    public string? JoinKey { get; set; }
    public bool Activated { get; set; } = true;
    public DateTimeOffset? CompletedAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public DateTimeOffset? RetryAt { get; set; }
    public string? LastError { get; set; }
}
