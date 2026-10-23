namespace SorteFederation.Models;

public partial class CompanyApplicationLicence : BaseEntity
{
    [Required]
    [EncryptColumn]
    public string LicenseKey { get; set; }

    [Required]
    [EncryptColumn]
    public string AppLicense { get; set; }

    public DateOnly IssuedAt { get; set; }

    public DateOnly? ExpiresAt { get; set; }

    public Guid ApplicationId { get; set; }
    [ForeignKey(nameof(ApplicationId))]
    public virtual ProjectApplication Application { get; set; }

    public Guid CompanyId { get; set; }
    [ForeignKey(nameof(CompanyId))]
    public virtual Company Company { get; set; }

    public LicenseStatus LicenseStatus { get; set; }
    public LicenseTypeEnum LicenseType { get; set; }
}
 