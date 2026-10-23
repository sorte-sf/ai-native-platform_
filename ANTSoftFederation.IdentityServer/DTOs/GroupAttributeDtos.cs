using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateGroupAttributeDto : BaseCreateDto
{
    public required Guid GroupId { get; set; }
    public required string Name { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class UpdateGroupAttributeDto : BaseUpdateDto
{
    public required Guid GroupId { get; set; }
    public required string Name { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class GroupAttributeDto : BaseDto
{
    public required Guid GroupId { get; set; }
    public required string Name { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class GroupAttributeListDto : BaseListDto
{
    public required string Name { get; set; }
    public string Value { get; set; } = string.Empty;
}