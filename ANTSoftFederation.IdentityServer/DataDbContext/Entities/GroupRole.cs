namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class GroupRole: EntityBase
{
    public Guid GroupId { get; set; }
    public virtual Group Group { get; set; }

    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; }
     
}
