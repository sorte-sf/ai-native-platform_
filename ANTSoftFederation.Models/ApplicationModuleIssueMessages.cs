namespace SorteFederation.Models;

public class ApplicationModuleIssueMessage : BaseEntity
{
    public required string Message { get; set; }

    public required Guid ApplicationModuleIssueId { get; set; }
    [ForeignKey(nameof(ApplicationModuleIssueId))]

    public virtual ApplicationModuleIssue ApplicationModuleIssue { get; set; } = null!;

    public bool IsCustomerMessage { get; set; }

    public Guid? ParentMessageId { get; set; }
    [ForeignKey(nameof(ParentMessageId))]

    public virtual ApplicationModuleIssueMessage ParentApplicationModuleIssueMessage { get; set; } = null!;

}
