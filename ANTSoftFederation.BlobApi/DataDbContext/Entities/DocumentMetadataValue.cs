
namespace SorteFederation.BlobApi.DataDbContext.Entities;


public class DocumentMetadataValue : BaseEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the document ID this metadata value belongs to
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the document template metadata field ID
    /// </summary>
    public Guid MetadataFieldId { get; set; }

    /// <summary>
    /// Gets or sets the value of the metadata field
    /// </summary>
    [StringLength(1000)]
    public string Value { get; set; } = string.Empty;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the document this metadata value belongs to
    /// </summary>
    public virtual Document Document { get; set; } = null!;

    /// <summary>
    /// Gets or sets the metadata field this value is for
    /// </summary>
    public virtual DocumentTemplateMetadataField MetadataField { get; set; } = null!;

    #endregion

    #region Methods

    /// <summary>
    /// Updates the metadata value
    /// </summary>
    /// <param name="value">The new value</param>
    public void UpdateValue(string value)
    {
        Value = value;
    }

    #endregion
}