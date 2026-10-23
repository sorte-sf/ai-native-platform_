
namespace SorteFederation.Models;

public class ApplicationModuleIssueEmbedding
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(1536)]
    public float[] IssueVector { get; set; }

    public required Guid ApplicationModuleIssueId { get; set; }
    [ForeignKey(nameof(ApplicationModuleIssueId))]
    public virtual ApplicationModuleIssue ApplicationModuleIssue { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public RecordState RecordState { get; set; } = RecordState.Active;
}
