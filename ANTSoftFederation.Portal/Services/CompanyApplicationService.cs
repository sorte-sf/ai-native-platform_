
namespace SorteFederation.ANTApplications.Services;

public class CompanyApplicationService : BaseService, ICompanyApplicationService
{
    private readonly ANTSoftPortalDbContext _context;

    public CompanyApplicationService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<CompanyApplicationListDto> CompanyApplications, int TotalCount)> GetCompanyApplicationsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.CompanyApplications.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(ca => ca.Note.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var companyApplications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ca => new CompanyApplicationListDto
            {
                Id = ca.Id,
                ContractDate = ca.ContractDate,
                InstallationDate = ca.InstallationDate,
                ApplicationName = ca.Application != null ? ca.Application.Name : string.Empty,
                CompanyName = ca.Company != null ? ca.Company.Name : string.Empty,
                TenantDatabaseType = ca.TenantDatabaseType,
                DataBaseType = ca.DataBaseType,
                CreatedAt = ca.CreatedAt,
                UpdatedAt = ca.UpdatedAt,
                IsActive = ca.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (companyApplications, totalCount);
    }

    public async Task<CompanyApplicationDto?> GetCompanyApplicationByIdAsync(Guid id)
    {
        var companyApplication = await _context.CompanyApplications
            .Where(ca => ca.Id == id)
            .Select(ca => new CompanyApplicationDto
            {
                Id = ca.Id,
                ContractDate = ca.ContractDate,
                InstallationDate = ca.InstallationDate,
                Note = ca.Note,
                ConnectionString = ca.ConnectionString,
                TenantDatabaseType = ca.TenantDatabaseType,
                DataBaseType = ca.DataBaseType,
                ApplicationId = ca.ApplicationId,
                CompanyId = ca.CompanyId,
                ApplicationName = ca.Application != null ? ca.Application.Name : string.Empty,
                CompanyName = ca.Company != null ? ca.Company.Name : string.Empty,
                CreatedAt = ca.CreatedAt,
                UpdatedAt = ca.UpdatedAt,
                CreatedBy = ca.CreatedBy,
                UpdatedBy = ca.UpdatedBy,
                IsActive = ca.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return companyApplication;
    }

    public async Task<Guid> CreateCompanyApplicationAsync(CreateCompanyApplicationDto createDto)
    {
        var companyApplication = new CompanyApplication
        {
            ContractDate = createDto.ContractDate,
            InstallationDate = createDto.InstallationDate,
            Note = createDto.Note,
            ConnectionString = createDto.ConnectionString,
            TenantDatabaseType = createDto.TenantDatabaseType,
            DataBaseType = createDto.DataBaseType,
            ApplicationId = createDto.ApplicationId,
            CompanyId = createDto.CompanyId,
            CreatedBy = createDto.CreatedBy
        };

        _context.CompanyApplications.Add(companyApplication);
        await _context.SaveChangesAsync();

        return companyApplication.Id;
    }

    public async Task<bool> UpdateCompanyApplicationAsync(Guid id, UpdateCompanyApplicationDto updateDto)
    {
        var companyApplication = await _context.CompanyApplications.FindAsync(id);
        
        if (companyApplication == null)
            return false;

        companyApplication.ContractDate = updateDto.ContractDate;
        companyApplication.InstallationDate = updateDto.InstallationDate;
        companyApplication.Note = updateDto.Note;
        companyApplication.ConnectionString = updateDto.ConnectionString;
        companyApplication.TenantDatabaseType = updateDto.TenantDatabaseType;
        companyApplication.DataBaseType = updateDto.DataBaseType;
        companyApplication.ApplicationId = updateDto.ApplicationId;
        companyApplication.CompanyId = updateDto.CompanyId;
        companyApplication.UpdatedBy = updateDto.UpdatedBy;
        companyApplication.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCompanyApplicationAsync(Guid id)
    {
        var companyApplication = await _context.CompanyApplications.FindAsync(id);
        
        if (companyApplication == null)
            return false;

        companyApplication.RecordState = RecordState.Deleted;
        companyApplication.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}