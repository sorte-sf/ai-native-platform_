
namespace SorteFederation.BlobApi.DataDbContext.Entities;


public class DocumentVersion : AuditableEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the version number
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Gets or sets the title of the document version
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the document version
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path or URL of the document version
    /// </summary>
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the document version
    /// </summary>
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checksum of the document version for integrity verification
    /// </summary>
    [StringLength(64)]
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is the current version
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Gets or sets the document ID this version belongs to
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the author ID who created this version
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the change summary for this version
    /// </summary>
    [StringLength(1000)]
    public string ChangeSummary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date this version was created
    /// </summary>
    public DateTime VersionDate { get; set; } = DateTime.UtcNow;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the document this version belongs to
    /// </summary>
    public virtual Document Document { get; set; } = null!;

    /// <summary>
    /// Gets or sets the author of this version
    /// </summary>
    public virtual string Author { get; set; } = null!;

    /// <summary>
    /// Gets or sets the approval workflow for this version
    /// </summary>
    public virtual DocumentVersionApproval? VersionApproval { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Marks this version as the current version
    /// </summary>
    public void SetAsCurrent()
    {
        IsCurrent = true;
    }

    /// <summary>
    /// Updates the change summary for this version
    /// </summary>
    /// <param name="summary">The change summary</param>
    public void UpdateChangeSummary(string summary)
    {
        ChangeSummary = summary;
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion
}