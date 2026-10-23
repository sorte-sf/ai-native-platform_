using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IRoleService
{
    Task<(IEnumerable<RoleListDto> Roles, int TotalCount)> GetRolesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<RoleDto?> GetRoleByIdAsync(Guid id);
    
    Task<Guid> CreateRoleAsync(CreateRoleDto createDto);
    
    Task<bool> UpdateRoleAsync(Guid id, UpdateRoleDto updateDto);
    
    Task<bool> DeleteRoleAsync(Guid id);
}