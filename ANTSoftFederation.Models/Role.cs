namespace SorteFederation.Models;

public class Role : BaseEntity
{ 
    [Required] 
    public string Name { get; set; }

    public string Description { get; set; }     

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = []; 
}