namespace SorteFederation.Models;

public partial class CompanyAppVersion : BaseEntity
{
    public Guid AppVersionId { get; set; }
    [ForeignKey(nameof(AppVersionId))]
    public virtual ApplicationVersioning AppVersion { get; set; }

    public Guid CompanyAppId { get; set; }
    [ForeignKey(nameof(CompanyAppId))]
    public virtual CompanyApplication CompanyApp { get; set; }
}
