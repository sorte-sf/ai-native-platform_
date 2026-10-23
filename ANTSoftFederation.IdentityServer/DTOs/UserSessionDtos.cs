using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateUserSessionDto : BaseCreateDto
{
    public required Guid UserId { get; set; }
    public required Guid ProjectId { get; set; }
    public required string SessionId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LastAccess { get; set; }
    public DateTime Started { get; set; }
    public string UserAgent { get; set; } = string.Empty;
}

public class UpdateUserSessionDto : BaseUpdateDto
{
    public required Guid UserId { get; set; }
    public required Guid ProjectId { get; set; }
    public required string SessionId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LastAccess { get; set; }
    public DateTime Started { get; set; }
    public string UserAgent { get; set; } = string.Empty;
}

public class UserSessionDto : BaseDto
{
    public required Guid UserId { get; set; }
    public required Guid ProjectId { get; set; }
    public required string SessionId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LastAccess { get; set; }
    public DateTime Started { get; set; }
    public string UserAgent { get; set; } = string.Empty;
}

public class UserSessionListDto : BaseListDto
{
    public required string SessionId { get; set; }
    public DateTime LastAccess { get; set; }
    public DateTime Started { get; set; }
}