namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderDashboardWidget : BuilderEntity
{
    public Guid DashboardId { get; set; }
    public required string Key { get; set; }
    public required string Type { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
    public int SortOrder { get; set; }
}
