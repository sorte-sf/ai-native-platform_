namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderConnectorCredential : BuilderEntity
{
    public Guid ConnectorId { get; set; }
    public required string SecretReference { get; set; }
    public required string Scheme { get; set; }
    public bool Active { get; set; } = true;
}
