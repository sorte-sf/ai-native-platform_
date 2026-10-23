namespace SorteFederation.Models;

public class UserSession
{
    [Key]
    public string SessionId { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    public string IpAddress { get; set; }

    public DateTime LastAccess { get; set; }
    public DateTime Started { get; set; }
    public string UserAgent { get; set; }
     
}