namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class GroupAttribute : EntityBase
{
    [Required]
    public Guid GroupId { get; set; }
    public virtual Group Group { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public string Value { get; set; }
}
