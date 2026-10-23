 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface ICompanyApplicationLicenceService
{
    Task<(IEnumerable<CompanyApplicationLicenceListDto> CompanyApplicationLicences, int TotalCount)> GetCompanyApplicationLicencesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<CompanyApplicationLicenceDto?> GetCompanyApplicationLicenceByIdAsync(Guid id);
    
    Task<Guid> CreateCompanyApplicationLicenceAsync(CreateCompanyApplicationLicenceDto createDto);
    
    Task<bool> UpdateCompanyApplicationLicenceAsync(Guid id, UpdateCompanyApplicationLicenceDto updateDto);
    
    Task<bool> DeleteCompanyApplicationLicenceAsync(Guid id);
}