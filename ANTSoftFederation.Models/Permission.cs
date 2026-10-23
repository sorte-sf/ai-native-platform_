namespace SorteFederation.Models;

public class Permission : BaseEntity
{
    [Required]
    public Guid ApplicationModuleId { get; set; }
    public virtual ApplicationModule ApplicationModule { get; set; }

    [Required]
    public string Name { get; set; }
     
    public string Description { get; set; }
     
    public bool IsSystemPermission { get; set; } = false;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    
}
