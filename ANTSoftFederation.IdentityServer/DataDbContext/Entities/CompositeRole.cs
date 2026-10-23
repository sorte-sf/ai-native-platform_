namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class CompositeRole: EntityBase
{
    public Guid ParentRoleId { get; set; }
    public virtual Role ParentRole { get; set; }

    public Guid ChildRoleId { get; set; }
    public virtual Role ChildRole { get; set; }
     
}