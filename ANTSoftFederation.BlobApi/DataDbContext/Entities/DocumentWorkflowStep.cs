
namespace SorteFederation.BlobApi.DataDbContext.Entities;

public class DocumentWorkflowStep : AuditableEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the workflow ID this step belongs to
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// Gets or sets the name of the workflow step
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the workflow step
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order of this step in the workflow
    /// </summary>
    public int StepOrder { get; set; }

    /// <summary>
    /// Gets or sets the role ID responsible for this step
    /// </summary>
    public Guid? ResponsibleRoleId { get; set; }

    /// <summary>
    /// Gets or sets the user ID responsible for this step (if specific user)
    /// </summary>
    public Guid? ResponsibleUserId { get; set; }

    /// <summary>
    /// Gets or sets the action required for this step
    /// </summary>
    [StringLength(100)]
    public string RequiredAction { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this step requires approval
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// Gets or sets the deadline in days for completing this step
    /// </summary>
    public int? DeadlineDays { get; set; }

    /// <summary>
    /// Gets or sets whether this step can be skipped
    /// </summary>
    public bool IsOptional { get; set; } = false;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the workflow this step belongs to
    /// </summary>
    public virtual DocumentWorkflow Workflow { get; set; } = null!;

    /// <summary>
    /// Gets or sets the role responsible for this step
    /// </summary>
    public virtual string ResponsibleRole { get; set; }

    /// <summary>
    /// Gets or sets the user responsible for this step
    /// </summary>
    public virtual string ResponsibleUser { get; set; }

    /// <summary>
    /// Gets or sets the workflow instances at this step
    /// </summary>
    public virtual ICollection<DocumentWorkflowInstance> WorkflowInstances { get; set; } = new List<DocumentWorkflowInstance>();

    /// <summary>
    /// Gets or sets the approvals for this step
    /// </summary>
    public virtual ICollection<DocumentWorkflowStepApproval> StepApprovals { get; set; } = new List<DocumentWorkflowStepApproval>();

    #endregion

    #region Methods

    /// <summary>
    /// Updates the step order
    /// </summary>
    /// <param name="stepOrder">The new step order</param>
    public void UpdateStepOrder(int stepOrder)
    {
        StepOrder = stepOrder;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the responsible role for this step
    /// </summary>
    /// <param name="roleId">The role ID</param>
    public void UpdateResponsibleRole(Guid? roleId)
    {
        ResponsibleRoleId = roleId;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the responsible user for this step
    /// </summary>
    /// <param name="userId">The user ID</param>
    public void UpdateResponsibleUser(Guid? userId)
    {
        ResponsibleUserId = userId;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the deadline for this step
    /// </summary>
    /// <param name="days">The number of days for the deadline</param>
    public void SetDeadline(int? days)
    {
        DeadlineDays = days;
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion

}