namespace SorteFederation.Builders.Data.Domain;

public class TenantTableMapping : BuilderEntity
{
    public string DestinationFieldName { get; set; } = string.Empty;
    public string SourceFieldName { get; set; } = string.Empty;
    public string? SourceFieldDescription { get; set; }
    public Guid TenantIntegrationConfigurationId { get; set; }
    public TenantIntegrationConfiguration TenantIntegrationConfiguration { get; set; } = null!;
}
 
