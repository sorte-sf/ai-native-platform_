using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateRolePermissionDto : BaseCreateDto
{
    public required Guid RoleId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class UpdateRolePermissionDto : BaseUpdateDto
{
    public required Guid RoleId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class RolePermissionDto : BaseDto
{
    public required Guid RoleId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class RolePermissionListDto : BaseListDto
{
    public required Guid RoleId { get; set; }
    public required Guid PermissionId { get; set; }
}