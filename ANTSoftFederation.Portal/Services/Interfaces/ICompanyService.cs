 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface ICompanyService
{
    Task<(IEnumerable<CompanyListDto> Companies, int TotalCount)> GetCompaniesAsync(int page = 1, int pageSize = 10, string? search = null);
    Task<CompanyDto?> GetCompanyByIdAsync(Guid id);    
    Task<Guid> CreateCompanyAsync(CreateCompanyDto createDto);    
    Task<bool> UpdateCompanyAsync(Guid id, UpdateCompanyDto updateDto);    
    Task<bool> DeleteCompanyAsync(Guid id);
}