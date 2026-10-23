using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateUserProjectDto : BaseCreateDto
{
    public required Guid UserId { get; set; }
    public required Guid ProjectId { get; set; }
}

public class UpdateUserProjectDto : BaseUpdateDto
{
    public required Guid UserId { get; set; }
    public required Guid ProjectId { get; set; }
}

public class UserProjectDto : BaseDto
{
    public required Guid UserId { get; set; }
    public required Guid ProjectId { get; set; }

    public string ProjectName { get; set; }
}

public class UserProjectListDto : BaseListDto
{
    public required Guid UserId { get; set; }
    public required Guid ProjectId { get; set; }

    public string ProjectName { get; set; }
}