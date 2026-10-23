using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderWorkflowInstance : BuilderEntity
{
    public Guid WorkflowId { get; set; }
    public Guid RecordId { get; set; }
    public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.Running;
    public string CurrentState { get; set; } = "Start";
    public DateTimeOffset? NextWakeAt { get; set; }
    public string? LastError { get; set; }
    public int RecoveryAttempts { get; set; }
    public ICollection<BuilderWorkflowTask> Tasks { get; set; } = [];
}
