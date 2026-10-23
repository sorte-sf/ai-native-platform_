
using System.ComponentModel.DataAnnotations.Schema;

namespace SorteFederation.Models;

public partial class ApplicationClaim : BaseEntity
{     
    public required string Name { get; set; }
     
    public required string Value { get; set; }

    public Guid? ParentId { get; set; }
    [ForeignKey(nameof(ParentId))]
    public virtual ApplicationClaim ParentClaim { get; set; } 

    public Guid ApplicationId { get; set; }
    [ForeignKey(nameof(ApplicationId))]
    public virtual ProjectApplication Application { get; set; }

}
