using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IUserProjectService
{
    Task<(ICollection<UserProjectListDto> UserProjects, int TotalCount)> GetUserProjectsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<UserProjectDto?> GetUserProjectByIdAsync(Guid userId, Guid projectId);
    
    Task<bool> CreateUserProjectAsync(CreateUserProjectDto createDto);
    
    Task<bool> UpdateUserProjectAsync(Guid userId, Guid projectId, UpdateUserProjectDto updateDto);
    
    Task<bool> DeleteUserProjectAsync(Guid userId, Guid projectId);
}