
namespace SorteFederation.ANTApplications.DTOs;

public class CreateApplicationClaimDto : BaseCreateDto
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public Guid? ParentId { get; set; }
    public required Guid ApplicationId { get; set; }
}

public class UpdateApplicationClaimDto : BaseUpdateDto
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public Guid? ParentId { get; set; }
    public required Guid ApplicationId { get; set; }
}

public class ApplicationClaimDto : BaseDto
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public Guid? ParentId { get; set; }
    public required Guid ApplicationId { get; set; }
}

public class ApplicationClaimListDto : BaseListDto
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public Guid? ParentId { get; set; }
    public required Guid ApplicationId { get; set; }
}