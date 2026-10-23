using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateUserPermissionDto : BaseCreateDto
{
    public required Guid UserId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class UpdateUserPermissionDto : BaseUpdateDto
{
    public required Guid UserId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class UserPermissionDto : BaseDto
{
    public required Guid UserId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class UserPermissionListDto : BaseListDto
{
    public required Guid UserId { get; set; }
    public required Guid PermissionId { get; set; }
}