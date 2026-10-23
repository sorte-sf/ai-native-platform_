
namespace SorteFederation.BlobApi.DataDbContext.Entities.Base;

public abstract class AuditableEntity : BaseEntity
{  
    public string CreatedBy { get; set; } = string.Empty;
     
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; 

    public string ModifiedBy { get; set; } = string.Empty;
     
    public DateTime? ModifiedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    public string? DeletedBy { get; set; } 

    public DateTime? DeletedDate { get; set; }
}