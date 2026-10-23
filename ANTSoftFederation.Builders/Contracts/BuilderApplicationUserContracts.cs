namespace SorteFederation.Builders.Contracts;

public sealed record BuilderAppUserLoginRequest(Guid TenantId, Guid ApplicationId, string Email, string Password);
public sealed record BuilderWorkspaceLoginRequest(string Email, string Password);
public sealed record UpdateBuilderWorkspaceAccessRequest(string? Role);
public sealed record CreateBuilderAppUserRequest(Guid TenantId, Guid ApplicationId, string Email, string DisplayName, string Password, string Role = "User");
public sealed record CreateTenantBuilderUserRequest(string Email, string DisplayName, string Password);
public sealed record AssignBuilderAppMembershipRequest(Guid UserId, string Role = "User");
public sealed record ChangeBuilderAppUserPasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
public sealed record BuilderAppUserLoginResponse(string AccessToken, DateTimeOffset ExpiresAt, object User, bool MustChangePassword);
