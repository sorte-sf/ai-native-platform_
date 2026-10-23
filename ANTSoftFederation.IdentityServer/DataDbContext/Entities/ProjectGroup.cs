using System.ComponentModel.DataAnnotations.Schema;

namespace ANTSoftFederation.IdentityServer.DataDbContext.Entities;

public class ProjectGroup
{
    public Guid ProjectId { get; set; }
    [ForeignKey(nameof(ProjectId))]
    public virtual Project Project { get; set; }

    public Guid GroupId { get; set; }
    [ForeignKey(nameof(GroupId))]
    public virtual Group Group { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RecordState RecordState { get; set; } = RecordState.Active;
}
