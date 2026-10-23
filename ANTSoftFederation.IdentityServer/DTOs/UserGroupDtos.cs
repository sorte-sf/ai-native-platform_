using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateUserGroupDto : BaseCreateDto
{
    public required Guid UserId { get; set; }
    public required Guid GroupId { get; set; }
}

public class UpdateUserGroupDto : BaseUpdateDto
{
    public required Guid UserId { get; set; }
    public required Guid GroupId { get; set; }
}

public class UserGroupDto : BaseDto
{
    public required Guid UserId { get; set; }
    public required Guid GroupId { get; set; }
}

public class UserGroupListDto : BaseListDto
{
    public required Guid UserId { get; set; }
    public required Guid GroupId { get; set; }
}