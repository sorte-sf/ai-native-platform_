namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class ClientRole : EntityBase
{
    public Guid ClientId { get; set; }
    public virtual Client Client { get; set; }

    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; }
     
}
