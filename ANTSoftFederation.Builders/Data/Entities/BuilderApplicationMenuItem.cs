namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderApplicationMenuItem : BuilderEntity
{
    public required Guid ApplicationId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? ModuleId { get; set; }
    public required string Key { get; set; }
    public required string Label { get; set; }
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool Visible { get; set; } = true;
    public string? PermissionKey { get; set; }
    public BuilderApplicationMenuItem? Parent { get; set; }
    public ICollection<BuilderApplicationMenuItem> Children { get; set; } = [];
}
