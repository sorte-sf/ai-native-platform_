using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IGroupService
{
    Task<(IEnumerable<GroupListDto> Groups, int TotalCount)> GetGroupsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<GroupDto?> GetGroupByIdAsync(Guid id);
    
    Task<Guid> CreateGroupAsync(CreateGroupDto createDto);
    
    Task<bool> UpdateGroupAsync(Guid id, UpdateGroupDto updateDto);
    
    Task<bool> DeleteGroupAsync(Guid id);
}