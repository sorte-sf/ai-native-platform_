namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderRecord : BuilderEntity
{
    public Guid ObjectId { get; set; }
    public string DataJson { get; set; } = "{}";
}
