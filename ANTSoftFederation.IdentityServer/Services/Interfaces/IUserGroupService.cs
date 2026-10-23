using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IUserGroupService
{
    Task<(IEnumerable<UserGroupListDto> UserGroups, int TotalCount)> GetUserGroupsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<UserGroupDto?> GetUserGroupByIdAsync(Guid userId, Guid groupId);
    
    Task<bool> CreateUserGroupAsync(CreateUserGroupDto createDto);
    
    Task<bool> UpdateUserGroupAsync(Guid userId, Guid groupId, UpdateUserGroupDto updateDto);
    
    Task<bool> DeleteUserGroupAsync(Guid userId, Guid groupId);
}