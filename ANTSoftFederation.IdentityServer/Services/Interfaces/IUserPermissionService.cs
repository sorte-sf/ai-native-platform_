using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IUserPermissionService
{
    Task<(IEnumerable<UserPermissionListDto> UserPermissions, int TotalCount)> GetUserPermissionsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<UserPermissionDto?> GetUserPermissionByIdAsync(Guid userId, Guid permissionId);
    
    Task<bool> CreateUserPermissionAsync(CreateUserPermissionDto createDto);
    
    Task<bool> UpdateUserPermissionAsync(Guid userId, Guid permissionId, UpdateUserPermissionDto updateDto);
    
    Task<bool> DeleteUserPermissionAsync(Guid userId, Guid permissionId);
}