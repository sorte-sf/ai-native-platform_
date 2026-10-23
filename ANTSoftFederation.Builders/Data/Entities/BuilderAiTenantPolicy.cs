namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderAiTenantPolicy : BuilderEntity
{
    public string Mode { get; set; } = "Disabled";
    public string Provider { get; set; } = "";
    public string? Endpoint { get; set; }
    public string? Model { get; set; }
    public string? ApiKeyReference { get; set; }
    public string AuthScheme { get; set; } = "bearer";
    public long MonthlyTokenLimit { get; set; } = 100_000;
    public int MonthlyRequestLimit { get; set; } = 1_000;
    public int MaxOutputTokens { get; set; } = 4_096;
}
