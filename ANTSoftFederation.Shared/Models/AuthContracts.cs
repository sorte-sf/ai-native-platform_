
namespace SorteFederation.Shared.Models;

public sealed record LoginRequest(string UserName, string Password);
public sealed record CurrentUserResponse(string Id, Guid? CompanyId, string UserName, string Role, string[] Permissions);
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, CurrentUserResponse User);
public sealed record RoleRequest(string Name, string Description, string[] Permissions);
public sealed record RoleResponse(Guid Id, string Name, string Description, string[] Permissions);
public sealed record UserRequest(string UserName, string DisplayName, string Password, Guid RoleId);
public sealed record UserUpdateRequest(string DisplayName, Guid RoleId, string? Password);
public sealed record UserResponse(Guid Id, string UserName, string DisplayName, Guid RoleId, string Role);
public class LoginRespondDto
{
    public bool Success { get; set; } = false;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public bool RequiresTwoFactor { get; set; } = false;
    public string? MessageCode { get; set; } = null;
    public string? Message { get; set; } = null;
    public int? ExpiresInSeconds { get; set; }
}