 
namespace SorteFederation.ANTApplications.DTOs;

public class CreateUserApplicationChatDto : BaseCreateDto
{
    public Guid? ParentId { get; set; }
    public required string Topic { get; set; }
    public string ChatDetails { get; set; } = string.Empty;
    public required Guid UserId { get; set; }
}

public class UpdateUserApplicationChatDto : BaseUpdateDto
{
    public Guid? ParentId { get; set; }
    public required string Topic { get; set; }
    public string ChatDetails { get; set; } = string.Empty;
    public required Guid UserId { get; set; }
}

public class UserApplicationChatDto : BaseDto
{
    public Guid? ParentId { get; set; }
    public required string Topic { get; set; }
    public string ChatDetails { get; set; } = string.Empty;
    public required Guid UserId { get; set; } 


}

public class UserApplicationChatListDto : BaseListDto
{
    public Guid? ParentId { get; set; }
    public required string Topic { get; set; }
    public required Guid UserId { get; set; } 
}