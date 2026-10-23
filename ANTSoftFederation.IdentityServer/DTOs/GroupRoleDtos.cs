using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateGroupRoleDto : BaseCreateDto
{
    public required Guid GroupId { get; set; }
    public required Guid RoleId { get; set; }
}

public class UpdateGroupRoleDto : BaseUpdateDto
{
    public required Guid GroupId { get; set; }
    public required Guid RoleId { get; set; }
}

public class GroupRoleDto : BaseDto
{
    public required Guid GroupId { get; set; }
    public required Guid RoleId { get; set; }
}

public class GroupRoleListDto : BaseListDto
{
    public required Guid GroupId { get; set; }
    public required Guid RoleId { get; set; }
}