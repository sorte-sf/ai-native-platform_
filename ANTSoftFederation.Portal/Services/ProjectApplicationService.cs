 
namespace SorteFederation.ANTApplications.Services;

public class ProjectApplicationService : BaseService, IProjectApplicationService
{
    private readonly ANTSoftPortalDbContext _context;

    public ProjectApplicationService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ProjectApplicationListDto> ProjectApplications, int TotalCount)> GetProjectApplicationsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ProjectApplications.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(pa => pa.Name.Contains(search) || pa.Descriptions.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var projectApplications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pa => new ProjectApplicationListDto
            {
                Id = pa.Id,
                Name = pa.Name,
                Descriptions = pa.Descriptions,
                AppKey = pa.AppKey,
                DbVersion = pa.DbVersion,
                CreatedAt = pa.CreatedAt,
                UpdatedAt = pa.UpdatedAt,
                IsActive = pa.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (projectApplications, totalCount);
    }

    public async Task<ProjectApplicationDto?> GetProjectApplicationByIdAsync(Guid id)
    {
        var projectApplication = await _context.ProjectApplications
            .Where(pa => pa.Id == id)
            .Select(pa => new ProjectApplicationDto
            {
                Id = pa.Id,
                Name = pa.Name,
                DefaultTheme = pa.DefaultTheme,
                DefaultLanguage = pa.DefaultLanguage,
                GlobalFontSize = pa.GlobalFontSize,
                ThemeColors = pa.ThemeColors,
                Descriptions = pa.Descriptions,
                AppKey = pa.AppKey,
                RealmKey = pa.RealmKey,
                SecretKey = pa.SecretKey,
                DbVersion = pa.DbVersion,
                AppLogo = pa.AppLogo,
                KeySalt = pa.KeySalt,
                AllowCrossOrigin = pa.AllowCrossOrigin,
                ConnectionString = pa.ConnectionString,
                DatabaseType = pa.DatabaseType,
                CreatedAt = pa.CreatedAt,
                UpdatedAt = pa.UpdatedAt,
                CreatedBy = pa.CreatedBy,
                UpdatedBy = pa.UpdatedBy,
                IsActive = pa.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return projectApplication;
    }

    public async Task<Guid> CreateProjectApplicationAsync(CreateProjectApplicationDto createDto)
    {
        var projectApplication = new ProjectApplication
        {
            Name = createDto.Name,
            DefaultTheme = createDto.DefaultTheme,
            DefaultLanguage = createDto.DefaultLanguage,
            GlobalFontSize = createDto.GlobalFontSize,
            ThemeColors = createDto.ThemeColors,
            Descriptions = createDto.Descriptions,
            AppKey = createDto.AppKey,
            RealmKey = createDto.RealmKey,
            SecretKey = createDto.SecretKey,
            DbVersion = createDto.DbVersion,
            AppLogo = createDto.AppLogo,
            KeySalt = createDto.KeySalt,
            AllowCrossOrigin = createDto.AllowCrossOrigin,
            ConnectionString = createDto.ConnectionString,
            DatabaseType = createDto.DatabaseType,
            CreatedBy = createDto.CreatedBy
        };

        _context.ProjectApplications.Add(projectApplication);
        await _context.SaveChangesAsync();

        return projectApplication.Id;
    }

    public async Task<bool> UpdateProjectApplicationAsync(Guid id, UpdateProjectApplicationDto updateDto)
    {
        var projectApplication = await _context.ProjectApplications.FindAsync(id);
        
        if (projectApplication == null)
            return false;

        projectApplication.Name = updateDto.Name;
        projectApplication.DefaultTheme = updateDto.DefaultTheme;
        projectApplication.DefaultLanguage = updateDto.DefaultLanguage;
        projectApplication.GlobalFontSize = updateDto.GlobalFontSize;
        projectApplication.ThemeColors = updateDto.ThemeColors;
        projectApplication.Descriptions = updateDto.Descriptions;
        projectApplication.AppKey = updateDto.AppKey;
        projectApplication.RealmKey = updateDto.RealmKey;
        projectApplication.SecretKey = updateDto.SecretKey;
        projectApplication.DbVersion = updateDto.DbVersion;
        projectApplication.AppLogo = updateDto.AppLogo;
        projectApplication.KeySalt = updateDto.KeySalt;
        projectApplication.AllowCrossOrigin = updateDto.AllowCrossOrigin;
        projectApplication.ConnectionString = updateDto.ConnectionString;
        projectApplication.DatabaseType = updateDto.DatabaseType;
        projectApplication.UpdatedBy = updateDto.UpdatedBy;
        projectApplication.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProjectApplicationAsync(Guid id)
    {
        var projectApplication = await _context.ProjectApplications.FindAsync(id);
        
        if (projectApplication == null)
            return false;

        projectApplication.RecordState = RecordState.Deleted;
        projectApplication.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

}