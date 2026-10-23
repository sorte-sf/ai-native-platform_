using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IClientRoleService
{
    Task<(IEnumerable<ClientRoleListDto> ClientRoles, int TotalCount)> GetClientRolesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ClientRoleDto?> GetClientRoleByIdAsync(Guid clientId, Guid roleId);
    
    Task<bool> CreateClientRoleAsync(CreateClientRoleDto createDto);
    
    Task<bool> UpdateClientRoleAsync(Guid clientId, Guid roleId, UpdateClientRoleDto updateDto);
    
    Task<bool> DeleteClientRoleAsync(Guid clientId, Guid roleId);
}