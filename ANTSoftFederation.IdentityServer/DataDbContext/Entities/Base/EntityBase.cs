using System.ComponentModel.DataAnnotations.Schema;

namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities.Base;

public class EntityBase
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string CreatedBy { get; set; }
    public virtual User CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UpdatedBy { get; set; }
    public virtual User UpdatedByUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Column(Order = 1040)]
    public RecordState RecordState { get; set; } = RecordState.Active;

    public string Tenant { get; set; } = "Internet";
}
