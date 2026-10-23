namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderUserGroup : BuilderEntity
{
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<BuilderUserGroupMembership> Members { get; set; } = [];
    public ICollection<BuilderGroupRoleAssignment> RoleAssignments { get; set; } = [];
}
