using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IClientService
{
    Task<(IEnumerable<ClientListDto> Clients, int TotalCount)> GetClientsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ClientDto?> GetClientByIdAsync(Guid id);
    
    Task<Guid> CreateClientAsync(CreateClientDto createDto);
    
    Task<bool> UpdateClientAsync(Guid id, UpdateClientDto updateDto);
    
    Task<bool> DeleteClientAsync(Guid id);
}