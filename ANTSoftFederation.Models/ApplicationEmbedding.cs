
namespace SorteFederation.Models;

public class ApplicationEmbedding
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(1536)]
    public float[] AppVector{ get; set; }

    public required Guid ApplicationDocId { get; set; }
    public virtual ApplicationDoc ApplicationDoc { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public RecordState RecordState { get; set; } = RecordState.Active;

}
