using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderWorkflow : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public DefinitionStatus Status { get; set; } = DefinitionStatus.Draft;
    public string DefinitionJson { get; set; } = "{}";
    public ICollection<BuilderWorkflowTransition> Transitions { get; set; } = [];
}
