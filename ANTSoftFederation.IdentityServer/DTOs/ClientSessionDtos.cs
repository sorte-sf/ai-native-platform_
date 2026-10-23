using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateClientSessionDto : BaseCreateDto
{
    public required Guid ClientId { get; set; }
    public required string SessionId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class UpdateClientSessionDto : BaseUpdateDto
{
    public required Guid ClientId { get; set; }
    public required string SessionId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class ClientSessionDto : BaseDto
{
    public required Guid ClientId { get; set; }
    public required string SessionId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class ClientSessionListDto : BaseListDto
{
    public required string SessionId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}