 
namespace SorteFederation.ANTApplications.DTOs;

public class CreateApplicationMenuDto : BaseCreateDto
{
    public required string Title { get; set; }
    public string SubTitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public ApplicationMenuType Type { get; set; }
    public int? Order { get; set; }
    public string[]? Roles { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid? ParentMenuId { get; set; }
}

public class UpdateApplicationMenuDto : BaseUpdateDto
{
    public required string Title { get; set; }
    public string SubTitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public ApplicationMenuType Type { get; set; }
    public int? Order { get; set; }
    public string[]? Roles { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid? ParentMenuId { get; set; }
}

public class ApplicationMenuDto : BaseDto
{
    public required string Title { get; set; }
    public string SubTitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public ApplicationMenuType Type { get; set; }
    public int? Order { get; set; }
    public string[]? Roles { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid? ParentMenuId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string ParentMenuTitle { get; set; } = string.Empty;
}

public class ApplicationMenuListDto : BaseListDto
{
    public required string Title { get; set; }
    public string SubTitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public ApplicationMenuType Type { get; set; }
    public int? Order { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
}