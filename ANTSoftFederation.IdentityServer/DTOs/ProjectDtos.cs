using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateProjectDto : BaseCreateDto
{
    public required string Name { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;

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
    public string PasswordPolicy { get; set; } = string.Empty;

    // Theme Settings
    public string LoginTheme { get; set; } = string.Empty;
    public string AccountTheme { get; set; } = string.Empty;
    public string AdminTheme { get; set; } = string.Empty;
    public string EmailTheme { get; set; } = string.Empty;
}

public class UpdateProjectDto : BaseUpdateDto
{
    public required string Name { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;

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
    public string PasswordPolicy { get; set; } = string.Empty;

    // Theme Settings
    public string LoginTheme { get; set; } = string.Empty;
    public string AccountTheme { get; set; } = string.Empty;
    public string AdminTheme { get; set; } = string.Empty;
    public string EmailTheme { get; set; } = string.Empty;
}

public class ProjectDto : BaseDto
{
    public required string Name { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;

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
    public string PasswordPolicy { get; set; } = string.Empty;

    // Theme Settings
    public string LoginTheme { get; set; } = string.Empty;
    public string AccountTheme { get; set; } = string.Empty;
    public string AdminTheme { get; set; } = string.Empty;
    public string EmailTheme { get; set; } = string.Empty;
}

public class ProjectListDto : BaseListDto
{
    public required string Name { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}