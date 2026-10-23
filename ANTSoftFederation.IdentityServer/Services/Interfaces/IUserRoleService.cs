using ANTSoftFederation.IdentityServer.DTOs; 

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IUserRoleService
{
    Task<(IEnumerable<UserRoleListDto> UserRoles, int TotalCount)> GetUserRolesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<UserRoleDto?> GetUserRoleByIdAsync(Guid userId, Guid roleId);
    
    Task<bool> CreateUserRoleAsync(CreateUserRoleDto createDto);
    
    Task<bool> UpdateUserRoleAsync(Guid userId, Guid roleId, UpdateUserRoleDto updateDto);
    
    Task<bool> DeleteUserRoleAsync(Guid userId, Guid roleId);
}