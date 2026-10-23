namespace SorteFederation.Models;

public partial class ApplicationIntegratorKey : BaseEntity
{     
    public string ApplicationKey { get; set; }

    public string SecretKey { get; set; }

    public Guid CompanyId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public virtual Company Company{ get; set; }

    public Guid ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public virtual ProjectApplication Application { get; set; }
     
}
