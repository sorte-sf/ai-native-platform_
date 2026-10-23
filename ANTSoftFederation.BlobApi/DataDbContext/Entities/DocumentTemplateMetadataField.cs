
namespace SorteFederation.BlobApi.DataDbContext.Entities;

public class DocumentTemplateMetadataField : BaseEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the document template ID this metadata field belongs to
    /// </summary>
    public Guid DocumentTemplateId { get; set; }

    /// <summary>
    /// Gets or sets the name of the metadata field
    /// </summary>
    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the metadata field
    /// </summary>
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type of the metadata field
    /// </summary>
    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this field is required
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this field is read-only
    /// </summary>
    public bool IsReadOnly { get; set; } = false;

    /// <summary>
    /// Gets or sets the default value for this field
    /// </summary>
    [StringLength(500)]
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation regex pattern for this field (nullable)
    /// </summary>
    [StringLength(500)]
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// Gets or sets the display order for this field
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets whether this field is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the help text for this field
    /// </summary>
    [StringLength(500)]
    public string HelpText { get; set; } = string.Empty;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the document template this metadata field belongs to
    /// </summary>
    public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;

    #endregion

    #region Methods
     
    public void Deactivate()
    {
        IsActive = false;
    }
     
    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Updates the default value for this field
    /// </summary>
    /// <param name="defaultValue">The new default value</param>
    public void UpdateDefaultValue(string defaultValue)
    {
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Updates the validation pattern for this field
    /// </summary>
    /// <param name="validationPattern">The new validation pattern</param>
    public void UpdateValidationPattern(string? validationPattern)
    {
        ValidationPattern = validationPattern;
    }

    #endregion
}