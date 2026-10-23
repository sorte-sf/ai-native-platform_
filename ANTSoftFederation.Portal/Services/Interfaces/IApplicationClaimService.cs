
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IApplicationClaimService
{
    Task<(IEnumerable<ApplicationClaimListDto> ApplicationClaims, int TotalCount)> GetApplicationClaimsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ApplicationClaimDto?> GetApplicationClaimByIdAsync(Guid id);
    
    Task<Guid> CreateApplicationClaimAsync(CreateApplicationClaimDto createDto);
    
    Task<bool> UpdateApplicationClaimAsync(Guid id, UpdateApplicationClaimDto updateDto);
    
    Task<bool> DeleteApplicationClaimAsync(Guid id);
}