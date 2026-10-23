namespace SorteFederation.Models;

public partial class Company: BaseEntity
{
    public Company()
    {
        ApplicationModuleIssues = new HashSet<ApplicationModuleIssue>();
        CompanyApplications = new HashSet<CompanyApplication>();
        ApplicationIntegratorKeys = new HashSet<ApplicationIntegratorKey>();
        CompanyApplicationLicences = new HashSet<CompanyApplicationLicence>();
    }
     
    public Guid? CompanyId { get; set; }

    [Required]
    public string Name { get; set; }

    [MaxLength(80)] public string RegistrationNo { get; set; }
    [MaxLength(256)] public string TaxOfficeName { get; set; }
    [MaxLength(80)] public string TaxNumber { get; set; }
    [MaxLength(256)] public string ContactPersonName { get; set; }

    [EncryptColumn]
    public string ContactPersonTelephoneNo { get; set; }

    [EncryptColumn]
    public string ContactPersonEmailAddress { get; set; }

    public bool? IsIntegrator { get; set; } = false;

    public Guid? CountryId { get; set; }
    public Country? Country { get; set; }

    public string Address{ get; set; }

    [MaxLength(500)] public string? LogoUrl { get; set; } = string.Empty;
    [MaxLength(20)] public string? ThemeKey { get; set; } = "default";
    [MaxLength(10)] public string? ThemeMode { get; set; } = "dark";

    public virtual ICollection<ApplicationModuleIssue> ApplicationModuleIssues { get; set; }
    public virtual ICollection<ApplicationIntegratorKey> ApplicationIntegratorKeys { get; set; }
    public virtual ICollection<CompanyApplication> CompanyApplications { get; set; }
    public virtual ICollection<CompanyApplicationLicence> CompanyApplicationLicences { get; set; }

}
