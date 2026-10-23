using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IGroupRoleService
{
    Task<(IEnumerable<GroupRoleListDto> GroupRoles, int TotalCount)> GetGroupRolesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<GroupRoleDto?> GetGroupRoleByIdAsync(Guid groupId, Guid roleId);
    
    Task<bool> CreateGroupRoleAsync(CreateGroupRoleDto createDto);
    
    Task<bool> UpdateGroupRoleAsync(Guid groupId, Guid roleId, UpdateGroupRoleDto updateDto);
    
    Task<bool> DeleteGroupRoleAsync(Guid groupId, Guid roleId);
}