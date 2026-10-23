
namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class ClientScope : EntityBase
{
    [Required]
    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    public ClientScopeType Type { get; set; } = ClientScopeType.Default;
    public string Protocol { get; set; } = "openid-connect";

    public bool DisplayOnConsentScreen { get; set; } = true;
    public string ConsentScreenText { get; set; }

    public virtual ICollection<ProtocolMapper> ProtocolMappers { get; set; } = new List<ProtocolMapper>();
}