using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateUserRoleDto : BaseCreateDto
{
    public required Guid UserId { get; set; }
    public required Guid RoleId { get; set; }
}

public class UpdateUserRoleDto : BaseUpdateDto
{
    public required Guid UserId { get; set; }
    public required Guid RoleId { get; set; }
}

public class UserRoleDto : BaseDto
{
    public required Guid UserId { get; set; }
    public required Guid RoleId { get; set; }
}

public class UserRoleListDto : BaseListDto
{
    public required Guid UserId { get; set; }
    public required Guid RoleId { get; set; }
}