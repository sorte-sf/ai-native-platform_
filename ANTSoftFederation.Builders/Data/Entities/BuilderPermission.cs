namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderPermission : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string ResourceType { get; set; }
    public required string Action { get; set; }
    /// <summary>Optional data object/table scope. Null means the whole application.</summary>
    public Guid? ObjectId { get; set; }
    /// <summary>Optional single-record scope within ObjectId.</summary>
    public Guid? RecordId { get; set; }
    public ICollection<BuilderPermissionGrant> Grants { get; set; } = [];
}
