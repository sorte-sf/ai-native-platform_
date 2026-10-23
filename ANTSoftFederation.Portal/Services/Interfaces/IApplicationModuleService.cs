 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IApplicationModuleService
{
    Task<(IEnumerable<ApplicationModuleListDto> ApplicationModules, int TotalCount)> GetApplicationModulesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ApplicationModuleDto?> GetApplicationModuleByIdAsync(Guid id);
    
    Task<Guid> CreateApplicationModuleAsync(CreateApplicationModuleDto createDto);
    
    Task<bool> UpdateApplicationModuleAsync(Guid id, UpdateApplicationModuleDto updateDto);
    
    Task<bool> DeleteApplicationModuleAsync(Guid id);
}