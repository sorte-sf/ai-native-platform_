 
namespace SorteFederation.ANTApplications.DTOs;

public class CreateCompanyApplicationLicenceDto : BaseCreateDto
{
    public required string LicenseKey { get; set; }
    public DateOnly IssuedAt { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public required Guid ApplicationId { get; set; }
    public required Guid CompanyId { get; set; }
    public LicenseStatus LicenseStatus { get; set; }
    public LicenseTypeEnum LicenseType { get; set; }
}

public class UpdateCompanyApplicationLicenceDto : BaseUpdateDto
{
    public required string LicenseKey { get; set; }
    public DateOnly IssuedAt { get; set; } 
    public DateOnly? ExpiresAt { get; set; }
    public required Guid ApplicationId { get; set; }
    public required Guid CompanyId { get; set; }
    public LicenseStatus LicenseStatus { get; set; }
    public LicenseTypeEnum LicenseType { get; set; }
}

public class CompanyApplicationLicenceDto : BaseDto
{
    public required string LicenseKey { get; set; }
    public DateOnly IssuedAt { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public required Guid ApplicationId { get; set; }
    public required Guid CompanyId { get; set; }
    public LicenseStatus LicenseStatus { get; set; }
    public LicenseTypeEnum LicenseType { get; set; }
}

public class CompanyApplicationLicenceListDto : BaseListDto
{
    public required string LicenseKey { get; set; }
    public DateOnly IssuedAt { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public required Guid ApplicationId { get; set; }
    public required Guid CompanyId { get; set; }
    public LicenseStatus LicenseStatus { get; set; }
    public LicenseTypeEnum LicenseType { get; set; }
}