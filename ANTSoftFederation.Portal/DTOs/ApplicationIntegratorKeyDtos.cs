 
namespace SorteFederation.ANTApplications.DTOs;

public class CreateApplicationIntegratorKeyDto : BaseCreateDto
{
    public required string ApplicationKey { get; set; }
    public required string SecretKey { get; set; }
    public required Guid CompanyId { get; set; }
    public required Guid ApplicationId { get; set; }
}

public class UpdateApplicationIntegratorKeyDto : BaseUpdateDto
{
    public required string ApplicationKey { get; set; }
    public required string SecretKey { get; set; }
    public required Guid CompanyId { get; set; }
    public required Guid ApplicationId { get; set; }
}

public class ApplicationIntegratorKeyDto : BaseDto
{
    public required string ApplicationKey { get; set; }
    public required string SecretKey { get; set; }
    public required Guid CompanyId { get; set; }
    public required Guid ApplicationId { get; set; }
}

public class ApplicationIntegratorKeyListDto : BaseListDto
{
    public required string ApplicationKey { get; set; }
    public required Guid CompanyId { get; set; }
    public required Guid ApplicationId { get; set; }
}