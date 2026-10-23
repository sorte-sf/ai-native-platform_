namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderUserGroupMembership : BuilderEntity
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public bool IsActive { get; set; } = true;
    public BuilderApplicationUser User { get; set; } = null!;
    public BuilderUserGroup Group { get; set; } = null!;
}
