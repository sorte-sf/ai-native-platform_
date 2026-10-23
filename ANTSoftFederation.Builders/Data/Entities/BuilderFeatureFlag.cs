namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderFeatureFlag : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public bool Enabled { get; set; }
    public string? RulesJson { get; set; }
}
