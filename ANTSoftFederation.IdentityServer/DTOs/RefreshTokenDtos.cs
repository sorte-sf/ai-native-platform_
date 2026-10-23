using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateRefreshTokenDto : BaseCreateDto
{
    public required Guid UserId { get; set; }
    public required Guid ClientId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Scope { get; set; } = string.Empty;
}

public class UpdateRefreshTokenDto : BaseUpdateDto
{
    public required Guid UserId { get; set; }
    public required Guid ClientId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Scope { get; set; } = string.Empty;
}

public class RefreshTokenDto : BaseDto
{
    public required Guid UserId { get; set; }
    public required Guid ClientId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Scope { get; set; } = string.Empty;
}

public class RefreshTokenListDto : BaseListDto
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}