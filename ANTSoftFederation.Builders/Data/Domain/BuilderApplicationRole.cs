namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderApplicationRole : BuilderEntity
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; } = true;
}
