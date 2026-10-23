namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class UserSession
{
    [Required]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    [Required]
    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; }

    public string SessionId { get; set; }
    public string IpAddress { get; set; }
    public DateTime LastAccess { get; set; }
    public DateTime Started { get; set; }
    public string UserAgent { get; set; }

    public virtual ICollection<ClientSession> ClientSessions { get; set; } = new List<ClientSession>();
}