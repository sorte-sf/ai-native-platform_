using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateCompositeRoleDto : BaseCreateDto
{
    public required Guid RoleId { get; set; }
    public required Guid CompositeRoleId { get; set; }
}

public class UpdateCompositeRoleDto : BaseUpdateDto
{
    public required Guid RoleId { get; set; }
    public required Guid CompositeRoleId { get; set; }
}

public class CompositeRoleDto : BaseDto
{
    public required Guid RoleId { get; set; }
    public required Guid CompositeRoleId { get; set; }
}

public class CompositeRoleListDto : BaseListDto
{
    public required Guid RoleId { get; set; }
    public required Guid CompositeRoleId { get; set; }
}