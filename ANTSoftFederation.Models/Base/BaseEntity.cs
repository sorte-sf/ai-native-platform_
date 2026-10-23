
namespace SorteFederation.Models;

public abstract class BaseEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public RecordState RecordState { get; set; } = RecordState.Active;

    public string Tenant { get; set; } = "Internet";
}

