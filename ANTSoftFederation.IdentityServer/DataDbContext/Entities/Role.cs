namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class Role : EntityBase
{
    [Required]
    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; }

    public Guid? ClientId { get; set; }
    public virtual Client Client { get; set; }

    [Required] 
    public string Name { get; set; }

    public string Description { get; set; }

    public bool Composite { get; set; } = false;

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
    public virtual ICollection<CompositeRole> CompositeRoles { get; set; } = [];
}