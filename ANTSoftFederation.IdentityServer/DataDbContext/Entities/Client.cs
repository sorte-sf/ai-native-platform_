
namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class Client : EntityBase
{
    [Required]
    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; }
    [Required] 
    public string ClientId { get; set; }
    [StringLength(500)]
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Enabled { get; set; } = true;
    public ClientProtocol Protocol { get; set; } = ClientProtocol.OpenIdConnect;    
    public ClientAccessType AccessType { get; set; } = ClientAccessType.Public;
    public string ClientSecret { get; set; }
    public bool ServiceAccountsEnabled { get; set; } = false;
    public bool AuthorizationServicesEnabled { get; set; } = false;
    public bool StandardFlowEnabled { get; set; } = true;
    public bool ImplicitFlowEnabled { get; set; } = false;
    public bool DirectAccessGrantsEnabled { get; set; } = true;
    public string RootUrl { get; set; }
    public string AdminUrl { get; set; }
    public string BaseUrl { get; set; }
    public List<string> RedirectUris { get; set; } = new List<string>();
    public List<string> WebOrigins { get; set; } = new List<string>();
    public int AccessTokenLifespan { get; set; } = 300;
    public bool AlwaysDisplayInConsole { get; set; } = false;
    public bool ConsentRequired { get; set; } = false;
    public bool DisplayOnConsentScreen { get; set; } = true;
    public string ConsentScreenText { get; set; }
    public bool PkceCodeChallengeMethod { get; set; } = false;
     
    public virtual ICollection<ClientScope> ClientScopes { get; set; } = new List<ClientScope>();
    public virtual ICollection<ClientRole> ClientRoles { get; set; } = new List<ClientRole>();
    public virtual ICollection<ProtocolMapper> ProtocolMappers { get; set; } = new List<ProtocolMapper>();

}
