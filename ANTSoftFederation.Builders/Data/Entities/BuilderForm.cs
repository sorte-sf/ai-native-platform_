using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderForm : BuilderEntity
{
    public Guid ObjectId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? LayoutJson { get; set; }
    public DefinitionStatus Status { get; set; } = DefinitionStatus.Draft;
}
