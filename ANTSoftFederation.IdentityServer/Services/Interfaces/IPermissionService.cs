using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IPermissionService
{
    Task<(IEnumerable<PermissionListDto> Permissions, int TotalCount)> GetPermissionsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<PermissionDto?> GetPermissionByIdAsync(Guid id);
    
    Task<Guid> CreatePermissionAsync(CreatePermissionDto createDto);
    
    Task<bool> UpdatePermissionAsync(Guid id, UpdatePermissionDto updateDto);
    
    Task<bool> DeletePermissionAsync(Guid id);
}