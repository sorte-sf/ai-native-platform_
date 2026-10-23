
namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateUserDto : BaseCreateDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public string TelePhone { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = false;
    public string? Password { get; set; }
    public bool IsOnline { get; set; } = false;
    public bool TemporaryPassword { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = false;
    public bool? IsLockedOut { get; set; } = false;
    public bool? AllowMultipleSessions { get; set; } = false;
    public bool? ChangePasswordOnFirstLogin { get; set; } = false;
    public TwoFactorType? TwoFactorType { get; set; }
}

public class UpdateUserDto : BaseUpdateDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public string TelePhone { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = false;
    public string? Password { get; set; }
    public bool IsOnline { get; set; } = false;
    public bool TemporaryPassword { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = false;
    public bool? IsLockedOut { get; set; } = false;
    public bool? AllowMultipleSessions { get; set; } = false;
    public bool? ChangePasswordOnFirstLogin { get; set; } = false;
    public TwoFactorType? TwoFactorType { get; set; }
}

public class UserDto : BaseDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public string TelePhone { get; set; } = string.Empty;
    public bool EmailVerified { get; set; } = false;
    public bool IsOnline { get; set; } = false;
    public DateTime? PasswordChangedAt { get; set; }
    public bool TemporaryPassword { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = false;
    public bool? IsLockedOut { get; set; } = false;
    public bool? AllowMultipleSessions { get; set; } = false;
    public bool? ChangePasswordOnFirstLogin { get; set; } = false;
    public TwoFactorType? TwoFactorType { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public class UserListDto : BaseListDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public bool IsOnline { get; set; } = false;
}