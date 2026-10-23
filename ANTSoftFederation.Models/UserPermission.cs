namespace SorteFederation.Models;

public class UserPermission : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public virtual User User{ get; set; }

    [Required]
    public Guid PermissionId { get; set; }
    public virtual Permission Permission { get; set; }
    
}
