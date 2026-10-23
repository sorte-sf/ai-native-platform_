using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IIdentityProviderConfigService
{
    Task<(IEnumerable<IdentityProviderConfigListDto> IdentityProviderConfigs, int TotalCount)> GetIdentityProviderConfigsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<IdentityProviderConfigDto?> GetIdentityProviderConfigByIdAsync(Guid id);
    
    Task<Guid> CreateIdentityProviderConfigAsync(CreateIdentityProviderConfigDto createDto);
    
    Task<bool> UpdateIdentityProviderConfigAsync(Guid id, UpdateIdentityProviderConfigDto updateDto);
    
    Task<bool> DeleteIdentityProviderConfigAsync(Guid id);
}