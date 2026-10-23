using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateProjectGroupDto : BaseCreateDto
{
    public required Guid ProjectId { get; set; }
    public required Guid GroupId { get; set; }
}

public class UpdateProjectGroupDto : BaseUpdateDto
{
    public required Guid ProjectId { get; set; }
    public required Guid GroupId { get; set; }
}

public class ProjectGroupDto : BaseDto
{
    public required Guid ProjectId { get; set; }
    public required Guid GroupId { get; set; }
}

public class ProjectGroupListDto : BaseListDto
{
    public required Guid ProjectId { get; set; }
    public required Guid GroupId { get; set; }
}