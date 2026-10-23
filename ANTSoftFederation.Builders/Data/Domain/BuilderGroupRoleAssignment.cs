namespace SorteFederation.Builders.Data.Domain;

/// <summary>A group receives a tenant role only within the selected application.</summary>
public sealed class BuilderGroupRoleAssignment : BuilderEntity
{
    public Guid GroupId { get; set; }
    public Guid RoleId { get; set; }
    public Guid ApplicationId { get; set; }
    public bool IsActive { get; set; } = true;
    public BuilderUserGroup Group { get; set; } = null!;
    public BuilderApplicationRole Role { get; set; } = null!;
}
