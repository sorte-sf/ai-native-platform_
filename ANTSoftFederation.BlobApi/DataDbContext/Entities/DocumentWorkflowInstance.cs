
namespace SorteFederation.BlobApi.DataDbContext.Entities;

public class DocumentWorkflowInstance : AuditableEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the document ID this workflow instance is for
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the workflow ID for this instance
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// Gets or sets the current step ID in the workflow
    /// </summary>
    public Guid? CurrentStepId { get; set; }

    /// <summary>
    /// Gets or sets the status of this workflow instance
    /// </summary>
    public DocumentWorkflowStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date the workflow was started
    /// </summary>
    public DateTime StartedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date the workflow was completed (nullable)
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Gets or sets the initiator ID of the workflow
    /// </summary>
    public Guid InitiatorId { get; set; }

    /// <summary>
    /// Gets or sets any notes about the workflow instance
    /// </summary>
    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the document this workflow instance is for
    /// </summary>
    public virtual Document Document { get; set; } = null!;

    /// <summary>
    /// Gets or sets the workflow for this instance
    /// </summary>
    public virtual DocumentWorkflow Workflow { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current step in the workflow
    /// </summary>
    public virtual DocumentWorkflowStep? CurrentStep { get; set; }

    /// <summary>
    /// Gets or sets the initiator of the workflow
    /// </summary>
    public virtual string Initiator { get; set; } = null!;

    /// <summary>
    /// Gets or sets the workflow step instances for this workflow
    /// </summary>
    public virtual ICollection<DocumentWorkflowStepInstance> StepInstances { get; set; } = new List<DocumentWorkflowStepInstance>();

    #endregion

    #region Methods

    /// <summary>
    /// Starts the workflow instance
    /// </summary>
    public void StartWorkflow()
    {
        Status = DocumentWorkflowStatus.InProgress;
        StartedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Completes the workflow instance
    /// </summary>
    public void CompleteWorkflow()
    {
        Status = DocumentWorkflowStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the workflow instance
    /// </summary>
    public void CancelWorkflow()
    {
        Status = DocumentWorkflowStatus.Cancelled;
        CompletedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Advances to the next step in the workflow
    /// </summary>
    /// <param name="nextStepId">The ID of the next step</param>
    public void AdvanceToNextStep(Guid nextStepId)
    {
        CurrentStepId = nextStepId;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a step instance to this workflow instance
    /// </summary>
    /// <param name="stepInstance">The step instance to add</param>
    public void AddStepInstance(DocumentWorkflowStepInstance stepInstance)
    {
        StepInstances.Add(stepInstance);
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion
}
 
public enum DocumentWorkflowStatus
{ 
    NotStarted=1, 
    InProgress, 
    Completed, 
    Cancelled, 
    OnHold
}