using System.ComponentModel.DataAnnotations.Schema;

namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class UserGroup
{
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }

    public Guid GroupId { get; set; }
    [ForeignKey(nameof(GroupId))]
    public virtual Group Group { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public RecordState RecordState { get; set; } = RecordState.Active;
}