 
namespace SorteFederation.ANTApplications.Services.Interfaces;

public interface IApplicationEmbeddingService
{
    Task<(IEnumerable<ApplicationEmbeddingListDto> ApplicationEmbeddings, int TotalCount)> GetApplicationEmbeddingsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<ApplicationEmbeddingDto?> GetApplicationEmbeddingByIdAsync(Guid id);
    
    Task<Guid> CreateApplicationEmbeddingAsync(CreateApplicationEmbeddingDto createDto);
    
    Task<bool> UpdateApplicationEmbeddingAsync(Guid id, UpdateApplicationEmbeddingDto updateDto);
    
    Task<bool> DeleteApplicationEmbeddingAsync(Guid id);
}