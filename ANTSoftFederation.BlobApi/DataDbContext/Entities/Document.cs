
namespace SorteFederation.BlobApi.DataDbContext.Entities;

public class Document : AuditableEntity
{
    #region Properties
     
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
     
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
     
    [StringLength(100)]
    public string DocumentType { get; set; } = string.Empty;
     
    public DocumentStatus Status { get; set; }
     
    public int CurrentVersionNumber { get; set; } = 1;
     
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
     
    public long FileSize { get; set; }
     
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;
     
    public DateTime? PublishedDate { get; set; }
     
    public DateTime? ExpiryDate { get; set; }
     
    public bool IsArchived { get; set; }
     
    public Guid? DocumentTemplateId { get; set; }
     
    public Guid OwnerId { get; set; }

    #endregion

    #region Navigation Properties
      
     
    public virtual DocumentTemplate? DocumentTemplate { get; set; }
     
    public virtual ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
     
    public virtual ICollection<DocumentAccessControl> AccessControls { get; set; } = new List<DocumentAccessControl>();
     
    public virtual ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
     
    public virtual ICollection<DocumentCategory> DocumentCategories { get; set; } = new List<DocumentCategory>();
     
    public virtual ICollection<DocumentWorkflowInstance> WorkflowInstances { get; set; } = new List<DocumentWorkflowInstance>();

    #endregion

    #region Methods
     
    public void UpdateStatus(DocumentStatus newStatus)
    {
        Status = newStatus;

        if (newStatus == DocumentStatus.Published)
        {
            PublishedDate = DateTime.UtcNow;
        }

        ModifiedDate = DateTime.UtcNow;
    }
     
    public void Archive()
    {
        IsArchived = true;
        ModifiedDate = DateTime.UtcNow;
    }
     
    public void Unarchive()
    {
        IsArchived = false;
        ModifiedDate = DateTime.UtcNow;
    }
     
    public void AddVersion(DocumentVersion version)
    {
        Versions.Add(version);
        CurrentVersionNumber = version.VersionNumber;
        ModifiedDate = DateTime.UtcNow;
    }
     
    public void AddAccessControl(DocumentAccessControl accessControl)
    {
        AccessControls.Add(accessControl);
        ModifiedDate = DateTime.UtcNow;
    }
     
    public void AddTag(DocumentTag tag)
    {
        DocumentTags.Add(tag);
        ModifiedDate = DateTime.UtcNow;
    } 

    public void AddCategory(DocumentCategory category)
    {
        DocumentCategories.Add(category);
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion
}

public enum DocumentStatus
{
    Draft = 1,
    UnderReview,
    Approved,
    Published,
    Rejected,
    Retired
}