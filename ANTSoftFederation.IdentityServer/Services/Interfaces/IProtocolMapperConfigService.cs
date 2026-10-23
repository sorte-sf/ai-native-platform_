using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IProtocolMapperConfigService
{
    Task<(IEnumerable<ProtocolMapperConfigListDto> ProtocolMapperConfigs, int TotalCount)> GetProtocolMapperConfigsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ProtocolMapperConfigDto?> GetProtocolMapperConfigByIdAsync(Guid id);
    
    Task<Guid> CreateProtocolMapperConfigAsync(CreateProtocolMapperConfigDto createDto);
    
    Task<bool> UpdateProtocolMapperConfigAsync(Guid id, UpdateProtocolMapperConfigDto updateDto);
    
    Task<bool> DeleteProtocolMapperConfigAsync(Guid id);
}