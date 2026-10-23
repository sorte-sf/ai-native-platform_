 
namespace ANTSoftFederation.IdentityServer.Services;

public class RoleService : BaseService, IRoleService
{
    private readonly ANTSoftIdentityDbContext _context;

    public RoleService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<RoleListDto> Roles, int TotalCount)> GetRolesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.Roles.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Name.Contains(search) || r.Description.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var roles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoleListDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Composite = r.Composite,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsActive = r.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (roles, totalCount);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid id)
    {
        var role = await _context.Roles
            .Where(r => r.Id == id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                ProjectId = r.ProjectId,
                ClientId = r.ClientId,
                Name = r.Name,
                Description = r.Description,
                Composite = r.Composite,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                CreatedBy = r.CreatedBy,
                UpdatedBy = r.UpdatedBy,
                IsActive = r.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return role;
    }

    public async Task<Guid> CreateRoleAsync(CreateRoleDto createDto)
    {
        var role = new Role
        {
            ProjectId = createDto.ProjectId,
            ClientId = createDto.ClientId,
            Name = createDto.Name,
            Description = createDto.Description,
            Composite = createDto.Composite,
            CreatedBy = createDto.CreatedBy
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return role.Id;
    }

    public async Task<bool> UpdateRoleAsync(Guid id, UpdateRoleDto updateDto)
    {
        var role = await _context.Roles.FindAsync(id);
        
        if (role == null)
            return false;

        role.ProjectId = updateDto.ProjectId;
        role.ClientId = updateDto.ClientId;
        role.Name = updateDto.Name;
        role.Description = updateDto.Description;
        role.Composite = updateDto.Composite;
        role.UpdatedBy = updateDto.UpdatedBy;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        
        if (role == null)
            return false;

        role.RecordState = RecordState.Deleted;
        role.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}