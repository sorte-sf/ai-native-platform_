using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderField : BuilderEntity
{
    public Guid ObjectId { get; set; }
    public required string Key { get; set; }
    public required string Label { get; set; }
    public BuilderFieldType Type { get; set; }
    public int SortOrder { get; set; }
    public bool Required { get; set; }
    public bool Searchable { get; set; }
    public bool ReadOnly { get; set; }
    public string? ConfigurationJson { get; set; }
}
