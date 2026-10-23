 
namespace ANTSoftFederation.IdentityServer.Services;

public class RolePermissionService : BaseService, IRolePermissionService
{
    private readonly ANTSoftIdentityDbContext _context;

    public RolePermissionService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<RolePermissionListDto> RolePermissions, int TotalCount)> GetRolePermissionsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.RolePermissions.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            // RolePermission has no searchable fields, so we'll skip search filter
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var rolePermissions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(rp => new RolePermissionListDto
            {
                Id = rp.Id,
                RoleId = rp.RoleId,
                PermissionId = rp.PermissionId,
                CreatedAt = rp.CreatedAt,
                UpdatedAt = rp.UpdatedAt,
                IsActive = rp.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (rolePermissions, totalCount);
    }

    public async Task<RolePermissionDto?> GetRolePermissionByIdAsync(Guid id)
    {
        var rolePermission = await _context.RolePermissions
            .Where(rp => rp.Id == id)
            .Select(rp => new RolePermissionDto
            {
                Id = rp.Id,
                RoleId = rp.RoleId,
                PermissionId = rp.PermissionId,
                CreatedAt = rp.CreatedAt,
                UpdatedAt = rp.UpdatedAt,
                CreatedBy = rp.CreatedBy,
                UpdatedBy = rp.UpdatedBy,
                IsActive = rp.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return rolePermission;
    }

    public async Task<Guid> CreateRolePermissionAsync(CreateRolePermissionDto createDto)
    {
        var rolePermission = new RolePermission
        {
            RoleId = createDto.RoleId,
            PermissionId = createDto.PermissionId,
            CreatedBy = createDto.CreatedBy
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();

        return rolePermission.Id;
    }

    public async Task<bool> UpdateRolePermissionAsync(Guid id, UpdateRolePermissionDto updateDto)
    {
        var rolePermission = await _context.RolePermissions.FindAsync(id);
        
        if (rolePermission == null)
            return false;

        rolePermission.RoleId = updateDto.RoleId;
        rolePermission.PermissionId = updateDto.PermissionId;
        rolePermission.UpdatedBy = updateDto.UpdatedBy;
        rolePermission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRolePermissionAsync(Guid id)
    {
        var rolePermission = await _context.RolePermissions.FindAsync(id);
        
        if (rolePermission == null)
            return false;

        rolePermission.RecordState = RecordState.Deleted;
        rolePermission.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}