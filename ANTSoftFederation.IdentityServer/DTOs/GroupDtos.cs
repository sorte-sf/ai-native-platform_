using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateGroupDto : BaseCreateDto
{
    public required Guid? ParentGroupId { get; set; }
    public required string Name { get; set; } 
}

public class UpdateGroupDto : BaseUpdateDto
{
    public required Guid? ParentGroupId { get; set; }
    public required string Name { get; set; }
}

public class GroupDto : BaseDto
{
    public required Guid? ParentGroupId { get; set; }
    public required string Name { get; set; }
    public string ParentName { get; set; } = string.Empty;
}

public class GroupListDto : BaseListDto
{
    public required string Name { get; set; }
    public string ParentName{ get; set; } = string.Empty;
}