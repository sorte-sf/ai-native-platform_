 
namespace SorteFederation.ANTApplications.DTOs;

public class CreateApplicationModuleIssueDto : BaseCreateDto
{
    public string Descriptions { get; set; } = string.Empty;
    public Guid? PrId { get; set; }
    public string Solutions { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public PriorityEnum? Priority { get; set; } = PriorityEnum.Low;
    public Guid ApplicationId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid ModuleId { get; set; }
    public TicketStatus TicketStatus { get; set; }
    public TicketType TicketType { get; set; }
    public int? CustomerSatisfaction { get; set; }
}

public class UpdateApplicationModuleIssueDto : BaseUpdateDto
{
    public string Descriptions { get; set; } = string.Empty;
    public Guid? PrId { get; set; }
    public string Solutions { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public PriorityEnum? Priority { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid ModuleId { get; set; }
    public TicketStatus TicketStatus { get; set; }
    public TicketType TicketType { get; set; }
    public int? CustomerSatisfaction { get; set; }
}

public class ApplicationModuleIssueDto : BaseDto
{
    public string Descriptions { get; set; } = string.Empty;
    public Guid? PrId { get; set; }
    public string Solutions { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public PriorityEnum? Priority { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid ModuleId { get; set; }
    public TicketStatus TicketStatus { get; set; }
    public TicketType TicketType { get; set; }
    public int? CustomerSatisfaction { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
}

public class ApplicationModuleIssueListDto : BaseListDto
{
    public string Descriptions { get; set; } = string.Empty;
    public PriorityEnum? Priority { get; set; }
    public TicketStatus TicketStatus { get; set; }
    public TicketType TicketType { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
}