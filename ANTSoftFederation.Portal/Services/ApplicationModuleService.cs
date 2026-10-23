
namespace SorteFederation.ANTApplications.Services;

public class ApplicationModuleService : BaseService, IApplicationModuleService
{
    private readonly ANTSoftPortalDbContext _context;

    public ApplicationModuleService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ApplicationModuleListDto> ApplicationModules, int TotalCount)> GetApplicationModulesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ApplicationModules.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(am => am.Name.Contains(search) || am.Descriptions.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var applicationModules = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(am => new ApplicationModuleListDto
            {
                Id = am.Id,
                Name = am.Name,
                Descriptions = am.Descriptions,
                ApplicationName = am.Application != null ? am.Application.Name : string.Empty,
                CreatedAt = am.CreatedAt,
                UpdatedAt = am.UpdatedAt,
                IsActive = am.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (applicationModules, totalCount);
    }

    public async Task<ApplicationModuleDto?> GetApplicationModuleByIdAsync(Guid id)
    {
        var applicationModule = await _context.ApplicationModules
            .Where(am => am.Id == id)
            .Select(am => new ApplicationModuleDto
            {
                Id = am.Id,
                Name = am.Name,
                Descriptions = am.Descriptions,
                ApplicationId = am.ApplicationId,
                ApplicationName = am.Application != null ? am.Application.Name : string.Empty,
                CreatedAt = am.CreatedAt,
                UpdatedAt = am.UpdatedAt,
                CreatedBy = am.CreatedBy,
                UpdatedBy = am.UpdatedBy,
                IsActive = am.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return applicationModule;
    }

    public async Task<Guid> CreateApplicationModuleAsync(CreateApplicationModuleDto createDto)
    {
        var applicationModule = new ApplicationModule
        {
            Name = createDto.Name,
            Descriptions = createDto.Descriptions,
            ApplicationId = createDto.ApplicationId,
            CreatedBy = createDto.CreatedBy
        };

        _context.ApplicationModules.Add(applicationModule);
        await _context.SaveChangesAsync();

        return applicationModule.Id;
    }

    public async Task<bool> UpdateApplicationModuleAsync(Guid id, UpdateApplicationModuleDto updateDto)
    {
        var applicationModule = await _context.ApplicationModules.FindAsync(id);
        
        if (applicationModule == null)
            return false;

        applicationModule.Name = updateDto.Name;
        applicationModule.Descriptions = updateDto.Descriptions;
        applicationModule.ApplicationId = updateDto.ApplicationId;
        applicationModule.UpdatedBy = updateDto.UpdatedBy;
        applicationModule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteApplicationModuleAsync(Guid id)
    {
        var applicationModule = await _context.ApplicationModules.FindAsync(id);
        
        if (applicationModule == null)
            return false;

        applicationModule.RecordState = RecordState.Deleted;
        applicationModule.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}