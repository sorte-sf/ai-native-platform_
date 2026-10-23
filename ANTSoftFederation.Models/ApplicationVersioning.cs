namespace SorteFederation.Models;

public partial class ApplicationVersioning : BaseEntity
{
    [Required]
    public string Version { get; set; }

    public string Note { get; set; }

    public Guid ApplicationId { get; set; }
    [ForeignKey(nameof(ApplicationId))]
    public virtual ProjectApplication Application { get; set; }

    public virtual ICollection<ApplicationDoc> ApplicationDocs { get; set; }

    public virtual ICollection<CompanyAppVersion> CompanyAppVersions { get; set; }
}
