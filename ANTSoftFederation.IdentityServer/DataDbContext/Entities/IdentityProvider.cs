namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class IdentityProvider : EntityBase
{
    [Required]
    public Guid RealmId { get; set; }
    public virtual Project Realm { get; set; }

    [Required]
    [StringLength(255)]
    public string Alias { get; set; }

    [StringLength(500)]
    public string DisplayName { get; set; }

    [Required]
    public string ProviderId { get; set; } // e.g., "google", "facebook", "saml"

    public bool Enabled { get; set; } = true;
    public bool StoreToken { get; set; } = false;
    public bool AddReadTokenRoleOnCreate { get; set; } = false;
    public bool TrustEmail { get; set; } = false;
    public bool LinkOnly { get; set; } = false;
    public string FirstBrokerLoginFlowAlias { get; set; }

    public virtual ICollection<IdentityProviderConfig> Config { get; set; } = new List<IdentityProviderConfig>();
}