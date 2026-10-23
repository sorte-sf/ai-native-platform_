namespace SorteFederation.Models;

public partial class CompanyApplication : BaseEntity
{
    public DateOnly? ContractDate { get; set; }
    public DateOnly? InstallationDate { get; set; }
    public string Note { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public TenantDatabaseType TenantDatabaseType { get; set; } = TenantDatabaseType.Internet;
    public DatabaseType DataBaseType { get; set; } = DatabaseType.PostgreSQL;
    public Guid ApplicationId { get; set; }
    [ForeignKey(nameof(ApplicationId))]
    public virtual ProjectApplication Application { get; set; }

    public Guid CompanyId { get; set; }
    [ForeignKey(nameof(CompanyId))]
    public virtual Company Company { get; set; }

    public virtual ICollection<CompanyAppVersion> CompanyAppVersions { get; set; }

    public virtual ICollection<CompanyApplicationLicence> CompanyApplicationLicences { get; set; }
}
