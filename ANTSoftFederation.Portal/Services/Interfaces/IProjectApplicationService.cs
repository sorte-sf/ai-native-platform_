 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IProjectApplicationService
{
    Task<(IEnumerable<ProjectApplicationListDto> ProjectApplications, int TotalCount)> GetProjectApplicationsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ProjectApplicationDto?> GetProjectApplicationByIdAsync(Guid id);
    
    Task<Guid> CreateProjectApplicationAsync(CreateProjectApplicationDto createDto);
    
    Task<bool> UpdateProjectApplicationAsync(Guid id, UpdateProjectApplicationDto updateDto);
    
    Task<bool> DeleteProjectApplicationAsync(Guid id);
}