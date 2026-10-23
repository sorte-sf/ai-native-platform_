namespace SorteFederation.Models;

public class RolePermission: BaseEntity
{
    [Required]
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; }

    [Required]
    public Guid PermissionId { get; set; }
    public virtual Permission Permission { get; set; } 
}
