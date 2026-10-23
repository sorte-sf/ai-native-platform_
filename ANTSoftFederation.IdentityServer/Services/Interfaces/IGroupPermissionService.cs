using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IGroupPermissionService
{
    Task<(IEnumerable<GroupPermissionListDto> GroupPermissions, int TotalCount)> GetGroupPermissionsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<GroupPermissionDto?> GetGroupPermissionByIdAsync(Guid groupId, Guid permissionId);
    
    Task<bool> CreateGroupPermissionAsync(CreateGroupPermissionDto createDto);
    
    Task<bool> UpdateGroupPermissionAsync(Guid groupId, Guid permissionId, UpdateGroupPermissionDto updateDto);
    
    Task<bool> DeleteGroupPermissionAsync(Guid groupId, Guid permissionId);
}