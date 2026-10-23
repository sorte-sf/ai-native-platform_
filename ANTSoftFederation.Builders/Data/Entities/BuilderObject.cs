using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderObject : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public DefinitionStatus Status { get; set; } = DefinitionStatus.Draft;
    public ICollection<BuilderField> Fields { get; set; } = [];
    public ICollection<BuilderForm> Forms { get; set; } = [];
}
