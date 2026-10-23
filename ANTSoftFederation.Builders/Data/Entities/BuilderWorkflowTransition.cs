namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderWorkflowTransition : BuilderEntity
{
    public Guid WorkflowId { get; set; }
    public required string FromState { get; set; }
    public required string ToState { get; set; }
    public required string Action { get; set; }
    public string? ConditionJson { get; set; }
}
