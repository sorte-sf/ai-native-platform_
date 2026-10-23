 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface ICompanyApplicationService
{
    Task<(IEnumerable<CompanyApplicationListDto> CompanyApplications, int TotalCount)> GetCompanyApplicationsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<CompanyApplicationDto?> GetCompanyApplicationByIdAsync(Guid id);
    
    Task<Guid> CreateCompanyApplicationAsync(CreateCompanyApplicationDto createDto);
    
    Task<bool> UpdateCompanyApplicationAsync(Guid id, UpdateCompanyApplicationDto updateDto);
    
    Task<bool> DeleteCompanyApplicationAsync(Guid id);
}