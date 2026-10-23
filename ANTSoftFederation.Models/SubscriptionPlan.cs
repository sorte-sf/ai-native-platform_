namespace SorteFederation.Models;

public sealed class SubscriptionPlan : BaseEntity
{
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    public int MaxApplications { get; set; }
    public int MaxUsers { get; set; }
    public int DurationMonths { get; set; }
    public decimal? Price { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public bool IsAvailable { get; set; } = true;
}
