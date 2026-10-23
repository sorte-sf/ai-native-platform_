using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreatePermissionDto : BaseCreateDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdatePermissionDto : BaseUpdateDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class PermissionDto : BaseDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class PermissionListDto : BaseListDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}