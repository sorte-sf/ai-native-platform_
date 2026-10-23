using EntityFrameworkCore.AutoHistory.Attributes;

namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

[ExcludeFromHistory]
public class RefreshToken
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string RevokedBy { get; set; }
    public string ReplacedByToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;
}
