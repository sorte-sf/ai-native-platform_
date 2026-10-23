namespace SorteFederation.Models;

public sealed class TenantSubscriptionPayment : BaseEntity
{
    public Guid TenantSubscriptionId { get; set; }
    [ForeignKey(nameof(TenantSubscriptionId))] public TenantSubscription Subscription { get; set; } = null!;
    public decimal Amount { get; set; }
    [Required, MaxLength(3)] public string Currency { get; set; } = "USD";
    public TenantSubscriptionPaymentStatus Status { get; set; } = TenantSubscriptionPaymentStatus.Pending;
    public DateTimeOffset? PaidAt { get; set; }
    [MaxLength(80)] public string? PaymentMethod { get; set; }
    [MaxLength(160)] public string? ProviderReference { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
}

public enum TenantSubscriptionPaymentStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}
