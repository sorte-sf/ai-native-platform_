 
namespace ANTSoftFederation.IdentityServer.Services;

public class PermissionService : BaseService, IPermissionService
{
    private readonly ANTSoftIdentityDbContext _context;

    public PermissionService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<PermissionListDto> Permissions, int TotalCount)> GetPermissionsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.Permissions.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var permissions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PermissionListDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (permissions, totalCount);
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(Guid id)
    {
        var permission = await _context.Permissions
            .Where(p => p.Id == id)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CreatedBy = p.CreatedBy,
                UpdatedBy = p.UpdatedBy,
                IsActive = p.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return permission;
    }

    public async Task<Guid> CreatePermissionAsync(CreatePermissionDto createDto)
    {
        var permission = new Permission
        {
            Name = createDto.Name,
            Description = createDto.Description,
            CreatedBy = createDto.CreatedBy
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        return permission.Id;
    }

    public async Task<bool> UpdatePermissionAsync(Guid id, UpdatePermissionDto updateDto)
    {
        var permission = await _context.Permissions.FindAsync(id);
        
        if (permission == null)
            return false;

        permission.Name = updateDto.Name;
        permission.Description = updateDto.Description;
        permission.UpdatedBy = updateDto.UpdatedBy;
        permission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePermissionAsync(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        
        if (permission == null)
            return false;

        permission.RecordState = RecordState.Deleted;
        permission.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}