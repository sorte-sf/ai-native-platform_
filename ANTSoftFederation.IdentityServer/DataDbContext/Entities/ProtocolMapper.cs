namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class ProtocolMapper : EntityBase
{ 
    public Guid? ClientId { get; set; }
    public virtual Client Client { get; set; }

    public Guid? ClientScopeId { get; set; }
    public virtual ClientScope ClientScope { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [Required]
    public string Protocol { get; set; } = "openid-connect";

    [Required]
    public string ProtocolMapperType { get; set; }

    public virtual ICollection<ProtocolMapperConfig> Config { get; set; } = new List<ProtocolMapperConfig>();
     
    public bool IsValid => ClientId.HasValue && !ClientScopeId.HasValue || !ClientId.HasValue && ClientScopeId.HasValue;
}