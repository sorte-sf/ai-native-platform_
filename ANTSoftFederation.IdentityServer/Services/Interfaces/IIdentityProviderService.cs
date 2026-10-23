using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IIdentityProviderService
{
    Task<(IEnumerable<IdentityProviderListDto> IdentityProviders, int TotalCount)> GetIdentityProvidersAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<IdentityProviderDto?> GetIdentityProviderByIdAsync(Guid id);
    
    Task<Guid> CreateIdentityProviderAsync(CreateIdentityProviderDto createDto);
    
    Task<bool> UpdateIdentityProviderAsync(Guid id, UpdateIdentityProviderDto updateDto);
    
    Task<bool> DeleteIdentityProviderAsync(Guid id);
}