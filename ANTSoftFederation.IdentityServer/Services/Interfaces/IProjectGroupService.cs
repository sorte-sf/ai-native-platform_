using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IProjectGroupService
{
    Task<(IEnumerable<ProjectGroupListDto> ProjectGroups, int TotalCount)> GetProjectGroupsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ProjectGroupDto?> GetProjectGroupByIdAsync(Guid projectId, Guid groupId);
    
    Task<bool> CreateProjectGroupAsync(CreateProjectGroupDto createDto);
    
    Task<bool> UpdateProjectGroupAsync(Guid projectId, Guid groupId, UpdateProjectGroupDto updateDto);
    
    Task<bool> DeleteProjectGroupAsync(Guid projectId, Guid groupId);
}