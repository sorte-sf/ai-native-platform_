using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateGroupPermissionDto : BaseCreateDto
{
    public required Guid GroupId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class UpdateGroupPermissionDto : BaseUpdateDto
{
    public required Guid GroupId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class GroupPermissionDto : BaseDto
{
    public required Guid GroupId { get; set; }
    public required Guid PermissionId { get; set; }
}

public class GroupPermissionListDto : BaseListDto
{
    public required Guid GroupId { get; set; }
    public required Guid PermissionId { get; set; }
}