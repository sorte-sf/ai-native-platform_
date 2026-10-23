namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderRule : BuilderEntity
{
    public required Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string DefinitionJson { get; set; }
    public bool Enabled { get; set; } = true;
}
