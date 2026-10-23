
namespace SorteFederation.BlobApi.DataDbContext.Entities;


public class DocumentTemplate : AuditableEntity
{
    #region Properties

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

 
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
     
    [Required]
    [StringLength(100)]
    public string DocumentType { get; set; } = string.Empty;
     
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
     
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;
     
    public bool IsActive { get; set; } = true;
     
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;
     
    public int Version { get; set; } = 1;
     
    public bool RequiresApproval { get; set; } = false;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the documents created from this template
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    /// <summary>
    /// Gets or sets the metadata fields associated with this template
    /// </summary>
    public virtual ICollection<DocumentTemplateMetadataField> MetadataFields { get; set; } = new List<DocumentTemplateMetadataField>();

    /// <summary>
    /// Gets or sets the workflow associated with this template
    /// </summary>
    public virtual DocumentWorkflow? AssociatedWorkflow { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Activates the template
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the template
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the template version
    /// </summary>
    public void UpdateVersion()
    {
        Version++;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a metadata field to the template
    /// </summary>
    /// <param name="metadataField">The metadata field to add</param>
    public void AddMetadataField(DocumentTemplateMetadataField metadataField)
    {
        MetadataFields.Add(metadataField);
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion
}