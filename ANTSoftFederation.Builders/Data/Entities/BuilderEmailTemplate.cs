namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderEmailTemplate : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required string Subject { get; set; }
    public string Language { get; set; } = "en";
    public string TemplateType { get; set; } = "Custom";
    public string DefinitionJson { get; set; } = "{\"schemaVersion\":1,\"blocks\":[]}";
}
