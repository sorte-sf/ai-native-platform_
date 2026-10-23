
namespace SorteFederation.ANTApplications.DTOs;

public class CreateCompanyAppVersionDto : BaseCreateDto
{
    public required Guid AppVersionId { get; set; }
    public required Guid CompanyAppId { get; set; }
}

public class UpdateCompanyAppVersionDto : BaseUpdateDto
{
    public required Guid AppVersionId { get; set; }
    public required Guid CompanyAppId { get; set; }
}

public class CompanyAppVersionDto : BaseDto
{
    public required Guid AppVersionId { get; set; }
    public required Guid CompanyAppId { get; set; }
}

public class CompanyAppVersionListDto : BaseListDto
{
    public required Guid AppVersionId { get; set; }
    public required Guid CompanyAppId { get; set; }
}