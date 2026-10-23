using ANTSoftFederation.IdentityServer.DTOs;
using ANTSoftFederation.IdentityServer.Services.Interfaces; 

namespace ANTSoftFederation.IdentityServer.Services;

public class UserRoleService : BaseService, IUserRoleService
{
    private readonly ANTSoftIdentityDbContext _context;

    public UserRoleService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<UserRoleListDto> UserRoles, int TotalCount)> GetUserRolesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.UserRoles.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            // UserRole has no searchable fields, so we'll skip search filter
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var userRoles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ur => new UserRoleListDto
            {
                //Id = ur.Id,
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                CreatedAt = ur.CreatedAt,
                UpdatedAt = ur.UpdatedAt,
                IsActive = ur.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (userRoles, totalCount);
    }

    public async Task<UserRoleDto?> GetUserRoleByIdAsync(Guid userId, Guid roleId)
    {
        var userRole = await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.RoleId == roleId)
            .Select(ur => new UserRoleDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                CreatedAt = ur.CreatedAt,
                UpdatedAt = ur.UpdatedAt,
                CreatedBy = ur.CreatedBy,
                UpdatedBy = ur.UpdatedBy,
                IsActive = ur.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return userRole;
    }

    public async Task<bool> CreateUserRoleAsync(CreateUserRoleDto createDto)
    {
        var userRole = new UserRole
        {
            UserId = createDto.UserId,
            RoleId = createDto.RoleId,
            CreatedBy = createDto.CreatedBy
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, Guid roleId, UpdateUserRoleDto updateDto)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        
        if (userRole == null)
            return false;

        userRole.UserId = updateDto.UserId;
        userRole.RoleId = updateDto.RoleId;
        userRole.UpdatedBy = updateDto.UpdatedBy;
        userRole.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserRoleAsync(Guid userId, Guid roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        
        if (userRole == null)
            return false;

        userRole.RecordState = RecordState.Deleted;
        userRole.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}