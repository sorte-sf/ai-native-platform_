using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IProtocolMapperService
{
    Task<(IEnumerable<ProtocolMapperListDto> ProtocolMappers, int TotalCount)> GetProtocolMappersAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ProtocolMapperDto?> GetProtocolMapperByIdAsync(Guid id);
    
    Task<Guid> CreateProtocolMapperAsync(CreateProtocolMapperDto createDto);
    
    Task<bool> UpdateProtocolMapperAsync(Guid id, UpdateProtocolMapperDto updateDto);
    
    Task<bool> DeleteProtocolMapperAsync(Guid id);
}