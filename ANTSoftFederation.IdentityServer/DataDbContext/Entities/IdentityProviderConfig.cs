namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class IdentityProviderConfig : EntityBase
{
    [Required]
    public Guid IdentityProviderId { get; set; }
    public virtual IdentityProvider IdentityProvider { get; set; }

    [Required]
    [StringLength(255)]
    public string Key { get; set; }

    public string Value { get; set; }
}
