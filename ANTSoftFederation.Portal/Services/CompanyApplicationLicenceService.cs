 
namespace SorteFederation.ANTApplications.Services;

public class CompanyApplicationLicenceService : BaseService, ICompanyApplicationLicenceService
{
    private readonly ANTSoftPortalDbContext _context;

    public CompanyApplicationLicenceService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<CompanyApplicationLicenceListDto> CompanyApplicationLicences, int TotalCount)> GetCompanyApplicationLicencesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.CompanyApplicationLicences.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(cal => cal.LicenseKey.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var companyApplicationLicences = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cal => new CompanyApplicationLicenceListDto
            {
                Id = cal.Id,
                LicenseKey = cal.LicenseKey,
                IssuedAt = cal.IssuedAt,
                ExpiresAt = cal.ExpiresAt,
                ApplicationId = cal.ApplicationId,
                CompanyId = cal.CompanyId,
                LicenseStatus = cal.LicenseStatus,
                LicenseType = cal.LicenseType,
                CreatedAt = cal.CreatedAt,
                UpdatedAt = cal.UpdatedAt,
                IsActive = cal.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (companyApplicationLicences, totalCount);
    }

    public async Task<CompanyApplicationLicenceDto?> GetCompanyApplicationLicenceByIdAsync(Guid id)
    {
        var companyApplicationLicence = await _context.CompanyApplicationLicences
            .Where(cal => cal.Id == id)
            .Select(cal => new CompanyApplicationLicenceDto
            {
                Id = cal.Id,
                LicenseKey = cal.LicenseKey,
                IssuedAt = cal.IssuedAt,
                ExpiresAt = cal.ExpiresAt,
                ApplicationId = cal.ApplicationId,
                CompanyId = cal.CompanyId,
                LicenseStatus = cal.LicenseStatus,
                LicenseType = cal.LicenseType,
                CreatedAt = cal.CreatedAt,
                UpdatedAt = cal.UpdatedAt,
                CreatedBy = cal.CreatedBy,
                UpdatedBy = cal.UpdatedBy,
                IsActive = cal.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return companyApplicationLicence;
    }

    public async Task<Guid> CreateCompanyApplicationLicenceAsync(CreateCompanyApplicationLicenceDto createDto)
    {
        var companyApplicationLicence = new CompanyApplicationLicence
        {
            LicenseKey = createDto.LicenseKey,
            IssuedAt = createDto.IssuedAt,
            ExpiresAt = createDto.ExpiresAt,
            ApplicationId = createDto.ApplicationId,
            CompanyId = createDto.CompanyId,
            LicenseStatus = createDto.LicenseStatus,
            LicenseType = createDto.LicenseType,
            CreatedBy = createDto.CreatedBy
        };

        _context.CompanyApplicationLicences.Add(companyApplicationLicence);
        await _context.SaveChangesAsync();

        return companyApplicationLicence.Id;
    }

    public async Task<bool> UpdateCompanyApplicationLicenceAsync(Guid id, UpdateCompanyApplicationLicenceDto updateDto)
    {
        var companyApplicationLicence = await _context.CompanyApplicationLicences.FindAsync(id);
        
        if (companyApplicationLicence == null)
            return false;

        companyApplicationLicence.LicenseKey = updateDto.LicenseKey;
        companyApplicationLicence.IssuedAt = updateDto.IssuedAt;
        companyApplicationLicence.ExpiresAt = updateDto.ExpiresAt;
        companyApplicationLicence.ApplicationId = updateDto.ApplicationId;
        companyApplicationLicence.CompanyId = updateDto.CompanyId;
        companyApplicationLicence.LicenseStatus = updateDto.LicenseStatus;
        companyApplicationLicence.LicenseType = updateDto.LicenseType;
        companyApplicationLicence.UpdatedBy = updateDto.UpdatedBy;
        companyApplicationLicence.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCompanyApplicationLicenceAsync(Guid id)
    {
        var companyApplicationLicence = await _context.CompanyApplicationLicences.FindAsync(id);
        
        if (companyApplicationLicence == null)
            return false;

        companyApplicationLicence.RecordState = RecordState.Deleted;
        companyApplicationLicence.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}