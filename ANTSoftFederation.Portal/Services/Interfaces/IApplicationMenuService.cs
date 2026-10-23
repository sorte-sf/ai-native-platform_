 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IApplicationMenuService
{
    Task<(IEnumerable<ApplicationMenuListDto> ApplicationMenus, int TotalCount)> GetApplicationMenusAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ApplicationMenuDto?> GetApplicationMenuByIdAsync(Guid id);
    
    Task<Guid> CreateApplicationMenuAsync(CreateApplicationMenuDto createDto);
    
    Task<bool> UpdateApplicationMenuAsync(Guid id, UpdateApplicationMenuDto updateDto);
    
    Task<bool> DeleteApplicationMenuAsync(Guid id);
}