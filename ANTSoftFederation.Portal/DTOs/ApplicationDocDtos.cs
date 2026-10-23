 
namespace SorteFederation.ANTApplications.DTOs;

public class CreateApplicationDocDto : BaseCreateDto
{
    public ApplicationDocType Type { get; set; } = ApplicationDocType.Technical;
    public required string Descriptions { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileLink { get; set; } = string.Empty;
    public string PhysicalFilePath { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Guid? ReviewedById { get; set; }
    public required Guid ApplicationId { get; set; }
    public Guid AppVersionId { get; set; }
}

public class UpdateApplicationDocDto : BaseUpdateDto
{
    public ApplicationDocType Type { get; set; } = ApplicationDocType.Technical;
    public required string Descriptions { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileLink { get; set; } = string.Empty;
    public string PhysicalFilePath { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Guid? ReviewedById { get; set; }
    public required Guid ApplicationId { get; set; }
    public Guid AppVersionId { get; set; }
}

public class ApplicationDocDto : BaseDto
{
    public ApplicationDocType Type { get; set; } = ApplicationDocType.Technical;
    public required string Descriptions { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileLink { get; set; } = string.Empty;
    public string PhysicalFilePath { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Guid? ReviewedById { get; set; }
    public required Guid ApplicationId { get; set; }
    public Guid AppVersionId { get; set; }
}

public class ApplicationDocListDto : BaseListDto
{
    public ApplicationDocType Type { get; set; } = ApplicationDocType.Technical;
    public required string Descriptions { get; set; }
    public string FileName { get; set; } = string.Empty;
    public required Guid ApplicationId { get; set; }
}