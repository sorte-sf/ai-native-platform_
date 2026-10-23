
namespace SorteFederation.BlobApi.DataDbContext.Entities;

public class DocumentAccessControl : AuditableEntity
{
    #region Properties

    public Guid DocumentId { get; set; }

    public string? UserId { get; set; }

    public Guid? RoleId { get; set; }

    public Guid? DepartmentId { get; set; }

    public DocumentPermissionLevel PermissionLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ExpirationDate { get; set; }

    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;

    #endregion

    #region Navigation Properties

    public virtual Document Document { get; set; } = null!;
     
    #endregion

    #region Methods

    public void Revoke()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }
    public void ExtendExpiration(DateTime newExpirationDate)
    {
        ExpirationDate = newExpirationDate;
        ModifiedDate = DateTime.UtcNow;
    }

    public void UpdatePermissionLevel(DocumentPermissionLevel newPermissionLevel)
    {
        PermissionLevel = newPermissionLevel;
        ModifiedDate = DateTime.UtcNow;
    }

    #endregion
}


public enum DocumentPermissionLevel
{
    None = 1,
    View,
    Comment,
    Edit,
    FullAccess
}