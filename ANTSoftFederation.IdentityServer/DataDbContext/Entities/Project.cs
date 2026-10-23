namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class Project : EntityBase
{
    [Required]
    public string Name { get; set; }

    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Logo { get; set; }

    public string SecretKey { get; set; }
    public string PublicKey { get; set; }

    // Security Settings
    public bool SslRequired { get; set; } = true;
    public bool LoginWithEmailAllowed { get; set; } = true;
    public bool DuplicateEmailsAllowed { get; set; } = false;
    public bool VerifyEmail { get; set; } = false;
    public bool RegistrationAllowed { get; set; } = false;
    public bool RegistrationEmailAsUsername { get; set; } = false;
    public bool RememberMe { get; set; } = false;
    public bool EditUsernameAllowed { get; set; } = false;
    public bool ResetPasswordAllowed { get; set; } = true;

    // Token Settings
    public int AccessTokenLifespan { get; set; } = 36000; // 5 minutes
    public int RefreshTokenMaxReuse { get; set; } = 0;
    public int SsoSessionIdleTimeout { get; set; } = 1800; // 30 minutes
    public int SsoSessionMaxLifespan { get; set; } = 36000; // 10 hours
    public int OfflineSessionIdleTimeout { get; set; } = 2592000; // 30 days

    // Password Policy
    public string PasswordPolicy { get; set; }

    // Theme Settings
    public string LoginTheme { get; set; }
    public string AccountTheme { get; set; }
    public string AdminTheme { get; set; }
    public string EmailTheme { get; set; }

    // Collections
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<ProjectGroup> ProjectGroups { get; set; } = new List<ProjectGroup>();
    public virtual ICollection<IdentityProvider> IdentityProviders { get; set; } = new List<IdentityProvider>();
}
