namespace SorteFederation.Builders.Data.Domain;

public partial class Tenant 
{
    [Key]
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public RecordState RecordState { get; set; }
    public bool HasActiveSubscription { get; set; }
    public int SubscriptionMaxApplications { get; set; }
    public int SubscriptionMaxUsers { get; set; }
    public DateOnly? SubscriptionStartsOn { get; set; }
    public DateOnly? SubscriptionEndsOn { get; set; }
    public virtual ICollection<BuilderApplication> BuilderApplications { get; set; } = [];
    public virtual ICollection<TenantIntegrationConfiguration> IntegrationConfigurations { get; set; } = [];
    public virtual TenantSmtpConfiguration? TenantSmtpConfiguration { get; set; }

}
