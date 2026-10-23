
namespace SorteFederation.BlobApi.DataDbContext.Entities;


public class DocumentCategory : AuditableEntity
{
    #region Properties
     
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
     
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
     
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
     
    public bool IsActive { get; set; } = true;
     
    public Guid? ParentCategoryId { get; set; }
     
    public int DisplayOrder { get; set; } 
    public bool RequiresApproval { get; set; } = false;

    #endregion

    #region Navigation Properties
     
    public virtual DocumentCategory? ParentCategory { get; set; } 
    public virtual ICollection<DocumentCategory> ChildCategories { get; set; } = new List<DocumentCategory>(); 
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    #endregion

    #region Methods
     
    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }
      
    public void SetParentCategory(Guid? parentCategoryId)
    {
        ParentCategoryId = parentCategoryId;
        ModifiedDate = DateTime.UtcNow;
    }
     
    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion
}