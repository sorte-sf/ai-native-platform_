 
namespace SorteFederation.ANTApplications.Services;

public class CompanyAppVersionService : BaseService, ICompanyAppVersionService
{
    private readonly ANTSoftPortalDbContext _context;

    public CompanyAppVersionService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<CompanyAppVersionListDto> CompanyAppVersions, int TotalCount)> GetCompanyAppVersionsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.CompanyAppVersions.AsQueryable();

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var companyAppVersions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cav => new CompanyAppVersionListDto
            {
                Id = cav.Id,
                AppVersionId = cav.AppVersionId,
                CompanyAppId = cav.CompanyAppId,
                CreatedAt = cav.CreatedAt,
                UpdatedAt = cav.UpdatedAt,
                IsActive = cav.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (companyAppVersions, totalCount);
    }

    public async Task<CompanyAppVersionDto?> GetCompanyAppVersionByIdAsync(Guid id)
    {
        var companyAppVersion = await _context.CompanyAppVersions
            .Where(cav => cav.Id == id)
            .Select(cav => new CompanyAppVersionDto
            {
                Id = cav.Id,
                AppVersionId = cav.AppVersionId,
                CompanyAppId = cav.CompanyAppId,
                CreatedAt = cav.CreatedAt,
                UpdatedAt = cav.UpdatedAt,
                CreatedBy = cav.CreatedBy,
                UpdatedBy = cav.UpdatedBy,
                IsActive = cav.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return companyAppVersion;
    }

    public async Task<Guid> CreateCompanyAppVersionAsync(CreateCompanyAppVersionDto createDto)
    {
        var companyAppVersion = new CompanyAppVersion
        {
            AppVersionId = createDto.AppVersionId,
            CompanyAppId = createDto.CompanyAppId,
            CreatedBy = createDto.CreatedBy
        };

        _context.CompanyAppVersions.Add(companyAppVersion);
        await _context.SaveChangesAsync();

        return companyAppVersion.Id;
    }

    public async Task<bool> UpdateCompanyAppVersionAsync(Guid id, UpdateCompanyAppVersionDto updateDto)
    {
        var companyAppVersion = await _context.CompanyAppVersions.FindAsync(id);
        
        if (companyAppVersion == null)
            return false;

        companyAppVersion.AppVersionId = updateDto.AppVersionId;
        companyAppVersion.CompanyAppId = updateDto.CompanyAppId;
        companyAppVersion.UpdatedBy = updateDto.UpdatedBy;
        companyAppVersion.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCompanyAppVersionAsync(Guid id)
    {
        var companyAppVersion = await _context.CompanyAppVersions.FindAsync(id);
        
        if (companyAppVersion == null)
            return false;

        companyAppVersion.RecordState = RecordState.Deleted;
        companyAppVersion.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}