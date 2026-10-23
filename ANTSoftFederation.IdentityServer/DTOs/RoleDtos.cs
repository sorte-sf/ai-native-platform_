using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateRoleDto : BaseCreateDto
{
    public required Guid ProjectId { get; set; }
    public Guid? ClientId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Composite { get; set; } = false;
}

public class UpdateRoleDto : BaseUpdateDto
{
    public required Guid ProjectId { get; set; }
    public Guid? ClientId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Composite { get; set; } = false;
}

public class RoleDto : BaseDto
{
    public required Guid ProjectId { get; set; }
    public Guid? ClientId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Composite { get; set; } = false;
}

public class RoleListDto : BaseListDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Composite { get; set; } = false;
}