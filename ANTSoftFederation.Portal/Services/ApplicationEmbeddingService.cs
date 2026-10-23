 
namespace SorteFederation.ANTApplications.Services;

public class ApplicationEmbeddingService : BaseService, IApplicationEmbeddingService
{
    private readonly ANTSoftPortalDbContext _context;

    public ApplicationEmbeddingService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ApplicationEmbeddingListDto> ApplicationEmbeddings, int TotalCount)> GetApplicationEmbeddingsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ApplicationEmbeddings.AsQueryable();

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var applicationEmbeddings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ae => new ApplicationEmbeddingListDto
            {
                Id = ae.Id,
                ApplicationDocId = ae.ApplicationDocId,
                CreatedAt = ae.CreatedAt,
                //UpdatedAt = ae.UpdatedAt,
                IsActive = ae.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (applicationEmbeddings, totalCount);
    }

    public async Task<ApplicationEmbeddingDto?> GetApplicationEmbeddingByIdAsync(Guid id)
    {
        var applicationEmbedding = await _context.ApplicationEmbeddings
            .Where(ae => ae.Id == id)
            .Select(ae => new ApplicationEmbeddingDto
            {
                Id = ae.Id,
                AppVector = ae.AppVector,
                ApplicationDocId = ae.ApplicationDocId,
                CreatedAt = ae.CreatedAt,
                IsActive = ae.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return applicationEmbedding;
    }

    public async Task<Guid> CreateApplicationEmbeddingAsync(CreateApplicationEmbeddingDto createDto)
    {
        var applicationEmbedding = new ApplicationEmbedding
        {
            AppVector = createDto.AppVector,
            ApplicationDocId = createDto.ApplicationDocId,
           // CreatedBy = createDto.CreatedBy
        };

        _context.ApplicationEmbeddings.Add(applicationEmbedding);
        await _context.SaveChangesAsync();

        return applicationEmbedding.Id;
    }

    public async Task<bool> UpdateApplicationEmbeddingAsync(Guid id, UpdateApplicationEmbeddingDto updateDto)
    {
        var applicationEmbedding = await _context.ApplicationEmbeddings.FindAsync(id);
        
        if (applicationEmbedding == null)
            return false;

        applicationEmbedding.AppVector = updateDto.AppVector;
        applicationEmbedding.ApplicationDocId = updateDto.ApplicationDocId;
        //applicationEmbedding.UpdatedBy = updateDto.UpdatedBy;
        //applicationEmbedding.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteApplicationEmbeddingAsync(Guid id)
    {
        var applicationEmbedding = await _context.ApplicationEmbeddings.FindAsync(id);
        
        if (applicationEmbedding == null)
            return false;

        applicationEmbedding.RecordState = RecordState.Deleted;
        //applicationEmbedding.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}