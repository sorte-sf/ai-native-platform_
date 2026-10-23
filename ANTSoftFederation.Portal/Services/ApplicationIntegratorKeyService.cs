 
namespace SorteFederation.ANTApplications.Services;

public class ApplicationIntegratorKeyService : BaseService, IApplicationIntegratorKeyService
{
    private readonly ANTSoftPortalDbContext _context;

    public ApplicationIntegratorKeyService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ApplicationIntegratorKeyListDto> ApplicationIntegratorKeys, int TotalCount)> GetApplicationIntegratorKeysAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ApplicationIntegratorKeys.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(aik => aik.ApplicationKey.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var applicationIntegratorKeys = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(aik => new ApplicationIntegratorKeyListDto
            {
                Id = aik.Id,
                ApplicationKey = aik.ApplicationKey,
                CompanyId = aik.CompanyId,
                ApplicationId = aik.ApplicationId,
                CreatedAt = aik.CreatedAt,
                UpdatedAt = aik.UpdatedAt,
                IsActive = aik.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (applicationIntegratorKeys, totalCount);
    }

    public async Task<ApplicationIntegratorKeyDto?> GetApplicationIntegratorKeyByIdAsync(Guid id)
    {
        var applicationIntegratorKey = await _context.ApplicationIntegratorKeys
            .Where(aik => aik.Id == id)
            .Select(aik => new ApplicationIntegratorKeyDto
            {
                Id = aik.Id,
                ApplicationKey = aik.ApplicationKey,
                SecretKey = aik.SecretKey,
                CompanyId = aik.CompanyId,
                ApplicationId = aik.ApplicationId,
                CreatedAt = aik.CreatedAt,
                UpdatedAt = aik.UpdatedAt,
                CreatedBy = aik.CreatedBy,
                UpdatedBy = aik.UpdatedBy,
                IsActive = aik.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return applicationIntegratorKey;
    }

    public async Task<Guid> CreateApplicationIntegratorKeyAsync(CreateApplicationIntegratorKeyDto createDto)
    {
        var applicationIntegratorKey = new ApplicationIntegratorKey
        {
            ApplicationKey = createDto.ApplicationKey,
            SecretKey = createDto.SecretKey,
            CompanyId = createDto.CompanyId,
            ApplicationId = createDto.ApplicationId,
            CreatedBy = createDto.CreatedBy
        };

        _context.ApplicationIntegratorKeys.Add(applicationIntegratorKey);
        await _context.SaveChangesAsync();

        return applicationIntegratorKey.Id;
    }

    public async Task<bool> UpdateApplicationIntegratorKeyAsync(Guid id, UpdateApplicationIntegratorKeyDto updateDto)
    {
        var applicationIntegratorKey = await _context.ApplicationIntegratorKeys.FindAsync(id);
        
        if (applicationIntegratorKey == null)
            return false;

        applicationIntegratorKey.ApplicationKey = updateDto.ApplicationKey;
        applicationIntegratorKey.SecretKey = updateDto.SecretKey;
        applicationIntegratorKey.CompanyId = updateDto.CompanyId;
        applicationIntegratorKey.ApplicationId = updateDto.ApplicationId;
        applicationIntegratorKey.UpdatedBy = updateDto.UpdatedBy;
        applicationIntegratorKey.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteApplicationIntegratorKeyAsync(Guid id)
    {
        var applicationIntegratorKey = await _context.ApplicationIntegratorKeys.FindAsync(id);
        
        if (applicationIntegratorKey == null)
            return false;

        applicationIntegratorKey.RecordState = RecordState.Deleted;
        applicationIntegratorKey.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}