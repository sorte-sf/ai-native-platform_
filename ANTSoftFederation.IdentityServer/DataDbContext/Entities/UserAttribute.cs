namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class UserAttribute
{
    [Required]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public string Value { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public RecordState RecordState { get; set; } = RecordState.Active;
}
