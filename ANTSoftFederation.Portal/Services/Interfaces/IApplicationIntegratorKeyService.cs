 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IApplicationIntegratorKeyService
{
    Task<(IEnumerable<ApplicationIntegratorKeyListDto> ApplicationIntegratorKeys, int TotalCount)> GetApplicationIntegratorKeysAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ApplicationIntegratorKeyDto?> GetApplicationIntegratorKeyByIdAsync(Guid id);
    
    Task<Guid> CreateApplicationIntegratorKeyAsync(CreateApplicationIntegratorKeyDto createDto);
    
    Task<bool> UpdateApplicationIntegratorKeyAsync(Guid id, UpdateApplicationIntegratorKeyDto updateDto);
    
    Task<bool> DeleteApplicationIntegratorKeyAsync(Guid id);
}