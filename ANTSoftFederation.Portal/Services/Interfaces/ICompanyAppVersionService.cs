 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface ICompanyAppVersionService
{
    Task<(IEnumerable<CompanyAppVersionListDto> CompanyAppVersions, int TotalCount)> GetCompanyAppVersionsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<CompanyAppVersionDto?> GetCompanyAppVersionByIdAsync(Guid id);
    
    Task<Guid> CreateCompanyAppVersionAsync(CreateCompanyAppVersionDto createDto);
    
    Task<bool> UpdateCompanyAppVersionAsync(Guid id, UpdateCompanyAppVersionDto updateDto);
    
    Task<bool> DeleteCompanyAppVersionAsync(Guid id);
}