
namespace SorteFederation.BlobApi.DataDbContext.Entities;

public class DocumentWorkflowStepInstance : AuditableEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the workflow instance ID this step instance belongs to
    /// </summary>
    public Guid WorkflowInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the workflow step ID this instance represents
    /// </summary>
    public Guid WorkflowStepId { get; set; }

    /// <summary>
    /// Gets or sets the status of this step instance
    /// </summary>
    public DocumentWorkflowStepStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date this step was started
    /// </summary>
    public DateTime StartedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date this step was completed (nullable)
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Gets or sets the assignee ID for this step (nullable)
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the date this step is due
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets any comments or notes for this step instance
    /// </summary>
    [StringLength(1000)]
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this step has been skipped
    /// </summary>
    public bool IsSkipped { get; set; } = false;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the workflow instance this step instance belongs to
    /// </summary>
    public virtual DocumentWorkflowInstance WorkflowInstance { get; set; } = null!;

    /// <summary>
    /// Gets or sets the workflow step this instance represents
    /// </summary>
    public virtual DocumentWorkflowStep WorkflowStep { get; set; } = null!;

    /// <summary>
    /// Gets or sets the assignee for this step
    /// </summary>
    public virtual string Assignee { get; set; }

    /// <summary>
    /// Gets or sets the approvals for this step instance
    /// </summary>
    public virtual ICollection<DocumentWorkflowStepApproval> StepApprovals { get; set; } = new List<DocumentWorkflowStepApproval>();

    #endregion

    #region Methods

    /// <summary>
    /// Starts the step instance
    /// </summary>
    public void StartStep()
    {
        Status = DocumentWorkflowStepStatus.InProgress;
        StartedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Completes the step instance
    /// </summary>
    public void CompleteStep()
    {
        Status = DocumentWorkflowStepStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Skips the step instance
    /// </summary>
    public void SkipStep()
    {
        Status = DocumentWorkflowStepStatus.Skipped;
        IsSkipped = true;
        CompletedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns the step to a user
    /// </summary>
    /// <param name="assigneeId">The ID of the assignee</param>
    public void AssignTo(Guid assigneeId)
    {
        AssigneeId = assigneeId;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the due date for this step
    /// </summary>
    /// <param name="dueDate">The due date</param>
    public void SetDueDate(DateTime dueDate)
    {
        DueDate = dueDate;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds comments to this step instance
    /// </summary>
    /// <param name="comments">The comments to add</param>
    public void AddComments(string comments)
    {
        Comments = comments;
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion
}
 
public enum DocumentWorkflowStepStatus
{ 
    NotStarted=1, 
    InProgress, 
    Completed, 
    Skipped, 
    OnHold
}