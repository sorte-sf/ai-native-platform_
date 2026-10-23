namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class Group : EntityBase
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public string Path { get; set; }

    public Guid? ParentGroupId { get; set; }
    public virtual Group ParentGroup { get; set; }

    public virtual ICollection<Group> SubGroups { get; set; } = new List<Group>();
    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public virtual ICollection<ProjectGroup> ProjectGroups { get; set; } = new List<ProjectGroup>();
    public virtual ICollection<GroupRole> GroupRoles { get; set; } = new List<GroupRole>();
    public virtual ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
    public virtual ICollection<GroupAttribute> GroupAttributes { get; set; } = new List<GroupAttribute>();

    
}
