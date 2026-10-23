
namespace ANTSoftFederation.IdentityServer.Services;

public class UserGroupService : BaseService, IUserGroupService
{
    private readonly ANTSoftIdentityDbContext _context;

    public UserGroupService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<UserGroupListDto> UserGroups, int TotalCount)> GetUserGroupsAsync(int page, int pageSize, string? search)
    {
        var query = _context.UserGroups.AsQueryable();

        // Apply search filter if needed (this is tricky for join entities)
        // For now, we'll just return all user groups with pagination
        var totalCount = await query.CountAsync();

        var userGroups = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ug => new UserGroupListDto
            {
                UserId = ug.UserId,
                GroupId = ug.GroupId,
                CreatedAt = ug.CreatedAt,
                UpdatedAt = ug.UpdatedAt,
                IsActive = ug.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (UserGroups: userGroups, TotalCount: totalCount);
    }

    public async Task<UserGroupDto?> GetUserGroupByIdAsync(Guid userId, Guid groupId)
    {
        var userGroup = await _context.UserGroups
            .Where(ug => ug.UserId == userId && ug.GroupId == groupId)
            .Select(ug => new UserGroupDto
            {
                UserId = ug.UserId,
                GroupId = ug.GroupId,
                CreatedAt = ug.CreatedAt,
                UpdatedAt = ug.UpdatedAt,
                CreatedBy = ug.CreatedBy,
                UpdatedBy = ug.UpdatedBy,
                IsActive = ug.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return userGroup;
    }

    public async Task<bool> CreateUserGroupAsync(CreateUserGroupDto createDto)
    {
        // Check if the user-group relationship already exists
        var existing = await _context.UserGroups
            .AnyAsync(ug => ug.UserId == createDto.UserId && ug.GroupId == createDto.GroupId);
            
        if (existing)
            return false;

        var userGroup = new UserGroup
        {
            UserId = createDto.UserId,
            GroupId = createDto.GroupId,
            CreatedBy = createDto.CreatedBy
        };

        _context.UserGroups.Add(userGroup);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateUserGroupAsync(Guid userId, Guid groupId, UpdateUserGroupDto updateDto)
    {
        var userGroup = await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);
        
        if (userGroup == null)
            return false;

        userGroup.UserId = updateDto.UserId;
        userGroup.GroupId = updateDto.GroupId;
        userGroup.UpdatedBy = updateDto.UpdatedBy;
        userGroup.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserGroupAsync(Guid userId, Guid groupId)
    {
        var userGroup = await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);
        
        if (userGroup == null)
            return false;

        userGroup.RecordState = RecordState.Deleted;
        userGroup.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}