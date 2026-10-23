 
namespace SorteFederation.ANTApplications.Services;

public class ApplicationMenuService : BaseService, IApplicationMenuService
{
    private readonly ANTSoftPortalDbContext _context;

    public ApplicationMenuService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ApplicationMenuListDto> ApplicationMenus, int TotalCount)> GetApplicationMenusAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ApplicationMenus.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(am => am.Title.Contains(search) || am.SubTitle.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var applicationMenus = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(am => new ApplicationMenuListDto
            {
                Id = am.Id,
                Title = am.Title,
                SubTitle = am.SubTitle,
                Icon = am.Icon,
                Type = am.Type,
                Order = am.Order,
                ApplicationName = am.Application != null ? am.Application.Name : string.Empty,
                CreatedAt = am.CreatedAt,
                UpdatedAt = am.UpdatedAt,
                IsActive = am.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (applicationMenus, totalCount);
    }

    public async Task<ApplicationMenuDto?> GetApplicationMenuByIdAsync(Guid id)
    {
        var applicationMenu = await _context.ApplicationMenus
            .Where(am => am.Id == id)
            .Select(am => new ApplicationMenuDto
            {
                Id = am.Id,
                Title = am.Title,
                SubTitle = am.SubTitle,
                Icon = am.Icon,
                Link = am.Link,
                Type = am.Type,
                Order = am.Order,
                Roles = am.Roles,
                ApplicationId = am.ApplicationId,
                ParentMenuId = am.ParentMenuId,
                ApplicationName = am.Application != null ? am.Application.Name : string.Empty,
                ParentMenuTitle = am.ParentMenu != null ? am.ParentMenu.Title : string.Empty,
                CreatedAt = am.CreatedAt,
                UpdatedAt = am.UpdatedAt,
                CreatedBy = am.CreatedBy,
                UpdatedBy = am.UpdatedBy,
                IsActive = am.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return applicationMenu;
    }

    public async Task<Guid> CreateApplicationMenuAsync(CreateApplicationMenuDto createDto)
    {
        var applicationMenu = new ApplicationMenu
        {
            Id = Guid.CreateVersion7(),
            Title = createDto.Title,
            SubTitle = createDto.SubTitle,
            Icon = createDto.Icon,
            Link = createDto.Link,
            Type = createDto.Type,
            Order = createDto.Order,
            Roles = createDto.Roles,
            ApplicationId = createDto.ApplicationId,
            ParentMenuId = createDto.ParentMenuId,
            CreatedBy = createDto.CreatedBy
        };

        _context.ApplicationMenus.Add(applicationMenu);
        await _context.SaveChangesAsync();

        return applicationMenu.Id;
    }

    public async Task<bool> UpdateApplicationMenuAsync(Guid id, UpdateApplicationMenuDto updateDto)
    {
        var applicationMenu = await _context.ApplicationMenus.FindAsync(id);
        
        if (applicationMenu == null)
            return false;

        applicationMenu.Title = updateDto.Title;
        applicationMenu.SubTitle = updateDto.SubTitle;
        applicationMenu.Icon = updateDto.Icon;
        applicationMenu.Link = updateDto.Link;
        applicationMenu.Type = updateDto.Type;
        applicationMenu.Order = updateDto.Order;
        applicationMenu.Roles = updateDto.Roles;
        applicationMenu.ApplicationId = updateDto.ApplicationId;
        applicationMenu.ParentMenuId = updateDto.ParentMenuId;
        applicationMenu.UpdatedBy = updateDto.UpdatedBy;
        applicationMenu.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteApplicationMenuAsync(Guid id)
    {
        var applicationMenu = await _context.ApplicationMenus.FindAsync(id);
        
        if (applicationMenu == null)
            return false;

        applicationMenu.RecordState = RecordState.Deleted;
        applicationMenu.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}