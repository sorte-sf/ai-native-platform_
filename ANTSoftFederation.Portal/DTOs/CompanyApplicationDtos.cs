 
namespace SorteFederation.ANTApplications.DTOs;

public class CreateCompanyApplicationDto : BaseCreateDto
{
    public DateOnly? ContractDate { get; set; }
    public DateOnly? InstallationDate { get; set; }
    public string Note { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public TenantDatabaseType TenantDatabaseType { get; set; } = TenantDatabaseType.Internet;
    public DatabaseType DataBaseType { get; set; } = DatabaseType.PostgreSQL;
    public Guid ApplicationId { get; set; }
    public Guid CompanyId { get; set; }
}

public class UpdateCompanyApplicationDto : BaseUpdateDto
{
    public DateOnly? ContractDate { get; set; }
    public DateOnly? InstallationDate { get; set; }
    public string Note { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public TenantDatabaseType TenantDatabaseType { get; set; }
    public DatabaseType DataBaseType { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid CompanyId { get; set; }
}

public class CompanyApplicationDto : BaseDto
{
    public DateOnly? ContractDate { get; set; }
    public DateOnly? InstallationDate { get; set; }
    public string Note { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public TenantDatabaseType TenantDatabaseType { get; set; }
    public DatabaseType DataBaseType { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid CompanyId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

public class CompanyApplicationListDto : BaseListDto
{
    public DateOnly? ContractDate { get; set; }
    public DateOnly? InstallationDate { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public TenantDatabaseType TenantDatabaseType { get; set; }
    public DatabaseType DataBaseType { get; set; }
}