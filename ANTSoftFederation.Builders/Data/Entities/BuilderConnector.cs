namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderConnector : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
    public bool Enabled { get; set; } = true;
}
