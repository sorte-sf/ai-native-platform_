using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateUserAttributeDto : BaseCreateDto
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class UpdateUserAttributeDto : BaseUpdateDto
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class UserAttributeDto : BaseDto
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class UserAttributeListDto : BaseListDto
{
    public required string Name { get; set; }
    public string Value { get; set; } = string.Empty;
}