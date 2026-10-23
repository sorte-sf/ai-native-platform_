namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderReport : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public Guid ObjectId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string DefinitionJson { get; set; } = "{}";
}
