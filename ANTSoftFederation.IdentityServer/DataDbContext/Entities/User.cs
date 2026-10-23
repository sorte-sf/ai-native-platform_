
namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class User : EntityBase
{ 
    [Required]
    [EncryptColumn]
    public string Username { get; set; }
    
    [EmailAddress]
    [EncryptColumn]
    public string Email { get; set; }
    
    [EncryptColumn]
    public string DisplayName { get; set; }

    [EncryptColumn]
    public string TelePhone { get; set; }
    public bool EmailVerified { get; set; } = false;
    public byte[]? Password { get; set; }
    public byte[]? Salt { get; set; }
    public bool IsOnline { get; set; } = false;
    public DateTime? PasswordChangedAt { get; set; }
    public bool TemporaryPassword { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = false;
    public bool IsSystemAccount { get; set; } = false;
    public bool? IsLockedOut { get; set; } = false;
    public bool? AllowMultipleSessions { get; set; } = false;
    public bool? ChangePasswordOnFirstLogin { get; set; } = false;
    public TwoFactorType? TwoFactorType { get; set; } = null;
    public DateTime? LastLoginDate { get; set; }
    public virtual ICollection<UserProject> UserProjects { get; set; } = [];
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<UserGroup> UserGroups { get; set; } = [];
    public virtual ICollection<UserSession> UserSessions { get; set; } = [];
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = [];
     

}
