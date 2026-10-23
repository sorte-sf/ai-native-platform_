 
namespace ANTSoftFederation.IdentityServer.Services;

public class UserPermissionService : BaseService, IUserPermissionService
{
    private readonly ANTSoftIdentityDbContext _context;

    public UserPermissionService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<UserPermissionListDto> UserPermissions, int TotalCount)> GetUserPermissionsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.UserPermissions.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            // UserPermission has no searchable fields, so we'll skip search filter
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var userPermissions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(up => new UserPermissionListDto
            {
                Id = up.Id,
                UserId = up.UserId,
                PermissionId = up.PermissionId,
                CreatedAt = up.CreatedAt,
                UpdatedAt = up.UpdatedAt,
                IsActive = up.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (userPermissions, totalCount);
    }

    public async Task<UserPermissionDto?> GetUserPermissionByIdAsync(Guid userId, Guid permissionId)
    {
        var userPermission = await _context.UserPermissions
            .Where(up => up.UserId == userId && up.PermissionId == permissionId)
            .Select(up => new UserPermissionDto
            {
                Id = up.Id,
                UserId = up.UserId,
                PermissionId = up.PermissionId,
                CreatedAt = up.CreatedAt,
                UpdatedAt = up.UpdatedAt,
                CreatedBy = up.CreatedBy,
                UpdatedBy = up.UpdatedBy,
                IsActive = up.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return userPermission;
    }

    public async Task<bool> CreateUserPermissionAsync(CreateUserPermissionDto createDto)
    {
        var userPermission = new UserPermission
        {
            UserId = createDto.UserId,
            PermissionId = createDto.PermissionId,
            CreatedBy = createDto.CreatedBy
        };

        _context.UserPermissions.Add(userPermission);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateUserPermissionAsync(Guid userId, Guid permissionId, UpdateUserPermissionDto updateDto)
    {
        var userPermission = await _context.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);
        
        if (userPermission == null)
            return false;

        userPermission.UserId = updateDto.UserId;
        userPermission.PermissionId = updateDto.PermissionId;
        userPermission.UpdatedBy = updateDto.UpdatedBy;
        userPermission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserPermissionAsync(Guid userId, Guid permissionId)
    {
        var userPermission = await _context.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);
        
        if (userPermission == null)
            return false;

        userPermission.RecordState = RecordState.Deleted;
        userPermission.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}