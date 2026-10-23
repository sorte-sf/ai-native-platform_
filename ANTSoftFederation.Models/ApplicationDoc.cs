using NpgsqlTypes;


namespace SorteFederation.Models;

public partial class ApplicationDoc : BaseEntity
{
    public ApplicationDocType Type { get; set; } = ApplicationDocType.Technical;

    public required string Descriptions { get; set; }

    public string FileName { get; set; }

    public string FileLink { get; set; }

    public string PhysicalFilePath { get; set; }

    public Guid? ParentId { get; set; }
    [ForeignKey(nameof(ParentId))]
    public virtual ApplicationDoc ParentDocument{ get; set; }

    public Guid? ReviewedById { get; set; }
    [ForeignKey(nameof(ReviewedById))]
    public virtual User User { get; set; }

    public DateTimeOffset? ReviewedDate { get; set; }

    public required Guid ApplicationId { get; set; }
    [ForeignKey(nameof(ApplicationId))]
    public virtual ProjectApplication Application { get; set; }

    public Guid AppVersionId { get; set; }
    [ForeignKey(nameof(AppVersionId))]
    public virtual ApplicationVersioning AppVersion { get; set; }

    public NpgsqlTsVector? SearchVector { get; set; }
}
