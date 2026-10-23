 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IApplicationModuleIssueService
{
    Task<(IEnumerable<ApplicationModuleIssueListDto> ApplicationModuleIssues, int TotalCount)> GetApplicationModuleIssuesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null,
        float[]? searchVector = null);

    Task<ApplicationModuleIssueDto?> GetApplicationModuleIssueByIdAsync(Guid id);
    
    Task<Guid> CreateApplicationModuleIssueAsync(CreateApplicationModuleIssueDto createDto);
    
    Task<bool> UpdateApplicationModuleIssueAsync(Guid id, UpdateApplicationModuleIssueDto updateDto);
    
    Task<bool> DeleteApplicationModuleIssueAsync(Guid id);
}