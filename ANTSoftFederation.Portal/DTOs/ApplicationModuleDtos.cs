 
namespace SorteFederation.ANTApplications.DTOs;

public class CreateApplicationModuleDto : BaseCreateDto
{
    public required string Name { get; set; }
    public string Descriptions { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
}

public class UpdateApplicationModuleDto : BaseUpdateDto
{
    public required string Name { get; set; }
    public string Descriptions { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
}

public class ApplicationModuleDto : BaseDto
{
    public required string Name { get; set; }
    public string Descriptions { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
}

public class ApplicationModuleListDto : BaseListDto
{
    public required string Name { get; set; }
    public string Descriptions { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
}