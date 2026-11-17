using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Data.Entities;

/// <summary>A reusable, tenant-scoped sequence of approval levels for workflow approval nodes.</summary>
public sealed class BuilderWorkflowApprovalPolicy : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string StepsJson { get; set; } = "[]";
}
