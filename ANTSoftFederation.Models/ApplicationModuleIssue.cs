
namespace SorteFederation.Models;

public partial class ApplicationModuleIssue : BaseEntity
{
    public string Descriptions { get; set; }

    public Guid? PrId { get; set; }

    public string Solutions { get; set; }

    public string Keywords { get; set; }

    public PriorityEnum? Priority { get; set; } = PriorityEnum.Low;
     
    public Guid ApplicationId { get; set; }
    [ForeignKey(nameof(ApplicationId))]
    public virtual ProjectApplication Application { get; set; }

    public Guid? CompanyId { get; set; }
    public virtual Company Company { get; set; }

    public Guid ModuleId { get; set; }
    [ForeignKey(nameof(ModuleId))]
    public virtual ApplicationModule ApplicationModule { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TicketStatus TicketStatus { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TicketType TicketType { get; set; }

    public int? CustomerSatisfaction { get; set; }

    public List<ApplicationModuleIssueMessage> ApplicationModuleIssueMessages { get; set; } = new();
}

public enum TicketStatus
{
    Open = 1,
    Closed,
    Suspended,
}

public enum TicketType
{
    Question = 1,
    Idea,
    Complaint,
    Returns,
}
