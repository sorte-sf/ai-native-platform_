namespace SorteFederation.Models;

public partial class ApplicationModule : BaseEntity
{     
    public required string Name { get; set; }
     
    public string Descriptions { get; set; }
     
    public required Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public virtual ProjectApplication Application { get; set; }

    public virtual ICollection<ApplicationModuleIssue> ApplicationModuleIssues { get; set; }
}
