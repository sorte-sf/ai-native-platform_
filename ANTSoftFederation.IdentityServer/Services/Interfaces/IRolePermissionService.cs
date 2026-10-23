using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IRolePermissionService
{
    Task<(IEnumerable<RolePermissionListDto> RolePermissions, int TotalCount)> GetRolePermissionsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<RolePermissionDto?> GetRolePermissionByIdAsync(Guid id);
    
    Task<Guid> CreateRolePermissionAsync(CreateRolePermissionDto createDto);
    
    Task<bool> UpdateRolePermissionAsync(Guid id, UpdateRolePermissionDto updateDto);
    
    Task<bool> DeleteRolePermissionAsync(Guid id);
}