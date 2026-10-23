using ANTSoftFederation.IdentityServer.DTOs.Base;
using ANTSoftFederation.IdentityServer.Enums;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateClientScopeDto : BaseCreateDto
{
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public ClientScopeType Type { get; set; } = ClientScopeType.Default;
}

public class UpdateClientScopeDto : BaseUpdateDto
{
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public ClientScopeType Type { get; set; } = ClientScopeType.Default;
}

public class ClientScopeDto : BaseDto
{
    public required Guid ProjectId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public ClientScopeType Type { get; set; } = ClientScopeType.Default;
}

public class ClientScopeListDto : BaseListDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}