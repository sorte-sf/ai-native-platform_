
namespace SorteFederation.BlobApi.DataDbContext.Entities;


public class DocumentTag : BaseEntity
{
    #region Properties

    /// <summary>
    /// Gets or sets the name of the tag
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the tag
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color code for the tag (for UI display)
    /// </summary>
    [StringLength(7)]
    public string ColorCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this tag is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the category of the tag
    /// </summary>
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Gets or sets the documents associated with this tag
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    #endregion

    #region Methods

    /// <summary>
    /// Deactivates the tag
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Updates the color code for the tag
    /// </summary>
    /// <param name="colorCode">The new color code</param>
    public void UpdateColor(string colorCode)
    {
        ColorCode = colorCode;
    }

    /// <summary>
    /// Updates the description of the tag
    /// </summary>
    /// <param name="description">The new description</param>
    public void UpdateDescription(string description)
    {
        Description = description;
    }

    #endregion
}