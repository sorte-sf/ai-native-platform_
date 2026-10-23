using SorteFederation.EncryptEFCoreColumn;

namespace SorteFederation.Builders.Data.Domain;

public class TenantIntegrationConfiguration : BuilderEntity
{
    public string Name { get; set; } = string.Empty;
    public int? Port { get; set; }
    [EncryptColumn]
    public string? UserName { get; set; }
    [EncryptColumn]
    public string? Password { get; set; }
    public bool? EnableSsl { get; set; } = false;
    [EncryptColumn]
    public string? Token { get; set; }
    [EncryptColumn]
    public string? ConnectionString { get; set; }
    public Tenant Tenant { get; set; } = null!;
    [EncryptColumn]
    public string? DatabaseName { get; set; }
    public Guid ApplicationId { get; set; }
    public BuilderApplication Application { get; set; } = null!;
    public IntegrationType IntegrationType { get; set; } = IntegrationType.DbServer;
    public virtual ICollection<TenantTableMapping> TenantTableMappings { get; set; } = [];
}

public enum IntegrationType
{
    DbServer = 1,
    Api,
    JsonFile,
    CsvFile,
}
