namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderPermissionGrant : BuilderEntity
{
    public Guid PermissionId { get; set; }
    public required string SubjectType { get; set; }
    public required string SubjectId { get; set; }
    public BuilderPermission Permission { get; set; } = null!;
}
