 

namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IApplicationDocService
{
    Task<(IEnumerable<ApplicationDocListDto> ApplicationDocs, int TotalCount)> GetApplicationDocsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null,
        float[]? searchVector = null);

    Task<ApplicationDocDto?> GetApplicationDocByIdAsync(Guid id);
    
    Task<Guid> CreateApplicationDocAsync(CreateApplicationDocDto createDto);
    
    Task<bool> UpdateApplicationDocAsync(Guid id, UpdateApplicationDocDto updateDto);
    
    Task<bool> DeleteApplicationDocAsync(Guid id);
}