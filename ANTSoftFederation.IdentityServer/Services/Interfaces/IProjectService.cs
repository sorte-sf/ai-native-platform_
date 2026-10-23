
namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IProjectService
{
    Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetProjectsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    
    Task<string> CreateProjectAsync(CreateProjectDto createDto);
    
    Task<bool> UpdateProjectAsync(Guid id, UpdateProjectDto updateDto);
    
    Task<bool> DeleteProjectAsync(Guid id);
}