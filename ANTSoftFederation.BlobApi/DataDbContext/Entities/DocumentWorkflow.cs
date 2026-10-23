
namespace SorteFederation.BlobApi.DataDbContext.Entities;


public class DocumentWorkflow : AuditableEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the name of the workflow
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the workflow
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this workflow is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the document type this workflow applies to
    /// </summary>
    [StringLength(100)]
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this workflow is the default for its document type
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the version of the workflow
    /// </summary>
    public int Version { get; set; } = 1;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the workflow steps for this workflow
    /// </summary>
    public virtual ICollection<DocumentWorkflowStep> WorkflowSteps { get; set; } = new List<DocumentWorkflowStep>();

    /// <summary>
    /// Gets or sets the document instances using this workflow
    /// </summary>
    public virtual ICollection<DocumentWorkflowInstance> WorkflowInstances { get; set; } = new List<DocumentWorkflowInstance>();

    /// <summary>
    /// Gets or sets the templates associated with this workflow
    /// </summary>
    public virtual ICollection<DocumentTemplate> AssociatedTemplates { get; set; } = new List<DocumentTemplate>();

    #endregion

    #region Methods

    /// <summary>
    /// Deactivates the workflow
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets this workflow as the default for its document type
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a workflow step to this workflow
    /// </summary>
    /// <param name="workflowStep">The workflow step to add</param>
    public void AddWorkflowStep(DocumentWorkflowStep workflowStep)
    {
        WorkflowSteps.Add(workflowStep);
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the workflow version
    /// </summary>
    public void UpdateVersion()
    {
        Version++;
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion
}