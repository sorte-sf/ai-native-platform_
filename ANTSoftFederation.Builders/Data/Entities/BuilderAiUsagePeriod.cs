namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderAiUsagePeriod
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public DateOnly PeriodStart { get; set; }
    public long UsedTokens { get; set; }
    public int Requests { get; set; }
    public long ReservedTokens { get; set; }
    public int ReservedRequests { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
