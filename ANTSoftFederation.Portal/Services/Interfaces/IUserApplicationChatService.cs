
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IUserApplicationChatService
{
    Task<(IEnumerable<UserApplicationChatListDto> UserApplicationChats, int TotalCount)> GetUserApplicationChatsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<UserApplicationChatDto?> GetUserApplicationChatByIdAsync(Guid id);
    
    Task<Guid> CreateUserApplicationChatAsync(CreateUserApplicationChatDto createDto);
    
    Task<bool> UpdateUserApplicationChatAsync(Guid id, UpdateUserApplicationChatDto updateDto);
    
    Task<bool> DeleteUserApplicationChatAsync(Guid id);
}