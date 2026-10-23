namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class ClientSession : EntityBase
{
    [Required]
    public Guid UserSessionId { get; set; }
    public virtual UserSession UserSession { get; set; }

    [Required]
    public Guid ClientId { get; set; }
    public virtual Client Client { get; set; }

    public string Action { get; set; }
    public DateTime Timestamp { get; set; }
    public string RedirectUri { get; set; }
}
