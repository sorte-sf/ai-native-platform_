using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderApplicationModule : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    // A module is a designed component, not just a navigation label.
    public Guid? ScreenId { get; set; }
    public Guid? ObjectId { get; set; }
    public Guid? FormId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DefinitionStatus Status { get; set; } = DefinitionStatus.Draft;
}
