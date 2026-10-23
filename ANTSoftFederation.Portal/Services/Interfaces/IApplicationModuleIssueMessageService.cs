 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IApplicationModuleIssueMessageService
{
    Task<(IEnumerable<ApplicationModuleIssueMessageListDto> ApplicationModuleIssueMessages, int TotalCount)> GetApplicationModuleIssueMessagesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ApplicationModuleIssueMessageDto?> GetApplicationModuleIssueMessageByIdAsync(Guid id);
    
    Task<Guid> CreateApplicationModuleIssueMessageAsync(CreateApplicationModuleIssueMessageDto createDto);
    
    Task<bool> UpdateApplicationModuleIssueMessageAsync(Guid id, UpdateApplicationModuleIssueMessageDto updateDto);
    
    Task<bool> DeleteApplicationModuleIssueMessageAsync(Guid id);
}