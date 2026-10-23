namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class ProtocolMapperConfig : EntityBase
{
    [Required]
    public Guid ProtocolMapperId { get; set; }
    public virtual ProtocolMapper ProtocolMapper { get; set; }

    [Required]
    [StringLength(255)]
    public string Key { get; set; }

    public string Value { get; set; }
}
