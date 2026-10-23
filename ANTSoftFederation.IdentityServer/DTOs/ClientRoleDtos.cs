using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateClientRoleDto : BaseCreateDto
{
    public required Guid ClientId { get; set; }
    public required Guid RoleId { get; set; }
}

public class UpdateClientRoleDto : BaseUpdateDto
{
    public required Guid ClientId { get; set; }
    public required Guid RoleId { get; set; }
}

public class ClientRoleDto : BaseDto
{
    public required Guid ClientId { get; set; }
    public required Guid RoleId { get; set; }
}

public class ClientRoleListDto : BaseListDto
{
    public required Guid ClientId { get; set; }
    public required Guid RoleId { get; set; }
}