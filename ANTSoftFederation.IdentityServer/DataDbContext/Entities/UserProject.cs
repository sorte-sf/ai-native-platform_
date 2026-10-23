namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class UserProject
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public RecordState RecordState { get; set; } = RecordState.Active;
}
