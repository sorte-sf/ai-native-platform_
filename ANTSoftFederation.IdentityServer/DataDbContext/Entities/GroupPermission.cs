namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class GroupPermission : EntityBase
{
    [Required]
    public Guid GroupId { get; set; }
    public virtual Group Group { get; set; }

    [Required]
    public Guid PermissionId { get; set; }
    public virtual Permission Permission { get; set; } 
}
