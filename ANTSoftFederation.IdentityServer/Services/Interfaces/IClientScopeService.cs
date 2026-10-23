using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IClientScopeService
{
    Task<(IEnumerable<ClientScopeListDto> ClientScopes, int TotalCount)> GetClientScopesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ClientScopeDto?> GetClientScopeByIdAsync(Guid id);
    
    Task<Guid> CreateClientScopeAsync(CreateClientScopeDto createDto);
    
    Task<bool> UpdateClientScopeAsync(Guid id, UpdateClientScopeDto updateDto);
    
    Task<bool> DeleteClientScopeAsync(Guid id);
}