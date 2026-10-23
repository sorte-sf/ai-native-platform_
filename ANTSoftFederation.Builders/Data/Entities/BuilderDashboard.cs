namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderDashboard : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string LayoutJson { get; set; } = "{}";
    public ICollection<BuilderDashboardWidget> Widgets { get; set; } = [];
}
