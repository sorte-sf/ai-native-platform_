namespace SorteFederation.Models;

public sealed class TenantSubscription : BaseEntity
{
    public Guid PortalTenantId { get; set; }
    [ForeignKey(nameof(PortalTenantId))] public PortalTenant PortalTenant { get; set; } = null!;
    public Guid SubscriptionPlanId { get; set; }
    [ForeignKey(nameof(SubscriptionPlanId))] public SubscriptionPlan Plan { get; set; } = null!;
    // Immutable commercial terms captured at assignment so later plan edits do not rewrite history.
    [MaxLength(120)] public string PlanName { get; set; } = string.Empty;
    public int MaxApplications { get; set; }
    public int MaxUsers { get; set; }
    public int DurationMonths { get; set; }
    public decimal? PlanPrice { get; set; }
    [MaxLength(3)] public string PlanCurrency { get; set; } = "USD";
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public TenantSubscriptionStatus Status { get; set; } = TenantSubscriptionStatus.Active;
    [MaxLength(1000)] public string? Notes { get; set; }
}

public enum TenantSubscriptionStatus
{
    Active = 1,
    Suspended = 2,
    Cancelled = 3,
    Expired = 4
}
