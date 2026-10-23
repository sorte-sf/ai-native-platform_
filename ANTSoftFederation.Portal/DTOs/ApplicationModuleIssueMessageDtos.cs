
namespace SorteFederation.ANTApplications.DTOs;

public class CreateApplicationModuleIssueMessageDto : BaseCreateDto
{
    public required string Message { get; set; }
    public required Guid ApplicationModuleIssueId { get; set; }
    public bool IsCustomerMessage { get; set; }
    public Guid? ParentMessageId { get; set; }
}

public class UpdateApplicationModuleIssueMessageDto : BaseUpdateDto
{
    public required string Message { get; set; }
    public required Guid ApplicationModuleIssueId { get; set; }
    public bool IsCustomerMessage { get; set; }
    public Guid? ParentMessageId { get; set; }
}

public class ApplicationModuleIssueMessageDto : BaseDto
{
    public required string Message { get; set; }
    public required Guid ApplicationModuleIssueId { get; set; }
    public bool IsCustomerMessage { get; set; }
    public Guid? ParentMessageId { get; set; }
}

public class ApplicationModuleIssueMessageListDto : BaseListDto
{
    public required string Message { get; set; }
    public required Guid ApplicationModuleIssueId { get; set; }
    public bool IsCustomerMessage { get; set; }
}