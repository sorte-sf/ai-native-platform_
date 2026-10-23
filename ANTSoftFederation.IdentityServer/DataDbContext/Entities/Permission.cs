namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class Permission : EntityBase
{
    [Required]
    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; }

    [Required]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
     
    public bool IsSystemPermission { get; set; } = false;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    
}
