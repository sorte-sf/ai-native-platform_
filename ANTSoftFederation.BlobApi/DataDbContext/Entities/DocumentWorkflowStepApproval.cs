
namespace SorteFederation.BlobApi.DataDbContext.Entities;

public class DocumentWorkflowStepApproval : AuditableEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the workflow step instance ID this approval is for
    /// </summary>
    public Guid WorkflowStepInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the approver ID
    /// </summary>
    public Guid ApproverId { get; set; }

    /// <summary>
    /// Gets or sets the approval status
    /// </summary>
    public DocumentApprovalStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date of the approval
    /// </summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>
    /// Gets or sets the comments for this approval
    /// </summary>
    [StringLength(1000)]
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is a required approval
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Gets or sets the order of this approval (for multiple approvers)
    /// </summary>
    public int ApprovalOrder { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the workflow step instance this approval is for
    /// </summary>
    public virtual DocumentWorkflowStepInstance WorkflowStepInstance { get; set; } = null!;

    /// <summary>
    /// Gets or sets the approver
    /// </summary>
    public virtual string Approver { get; set; } = null!;

    #endregion

    #region Methods

    /// <summary>
    /// Approves the workflow step
    /// </summary>
    /// <param name="comments">Optional comments</param>
    public void Approve(string? comments = null)
    {
        Status = DocumentApprovalStatus.Approved;
        ApprovalDate = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(comments))
        {
            Comments = comments;
        }
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Rejects the workflow step
    /// </summary>
    /// <param name="comments">Required rejection comments</param>
    public void Reject(string comments)
    {
        Status = DocumentApprovalStatus.Rejected;
        ApprovalDate = DateTime.UtcNow;
        Comments = comments;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the approval order
    /// </summary>
    /// <param name="order">The new approval order</param>
    public void UpdateApprovalOrder(int order)
    {
        ApprovalOrder = order;
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion
}
 
public enum DocumentApprovalStatus
{ 
    Pending=1, 
    Approved, 
    Rejected
}