namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderApplicationScreen : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Key { get; set; }
    public required string Title { get; set; }
    public required string HtmlContent { get; set; }
    public Guid? ObjectId { get; set; }
    public string? FormKey { get; set; }
    public Guid? FormId { get; set; }
    public string? DefinitionJson { get; set; }
}
