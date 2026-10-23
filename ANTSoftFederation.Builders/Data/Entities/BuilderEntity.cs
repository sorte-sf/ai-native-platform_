using SorteFederation.GlobalEnums;

namespace SorteFederation.Builders.Data.Domain;

public abstract class BuilderEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int Version { get; set; } = 1;
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }
    public RecordState RecordState { get; set; } = RecordState.Active;
}
