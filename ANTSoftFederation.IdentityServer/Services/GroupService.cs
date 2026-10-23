 
namespace ANTSoftFederation.IdentityServer.Services;

public class GroupService : BaseService, IGroupService
{
    private readonly ANTSoftIdentityDbContext _context;
    private readonly IMemoryCache _cache;

    public GroupService(ANTSoftIdentityDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<(IEnumerable<GroupListDto> Groups, int TotalCount)> GetGroupsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.Groups.AsNoTracking().AsQueryable();
         
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(g => g.Name.Contains(search));
        }
         
        var totalCount = await query.CountAsync();
         
        var groups = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GroupListDto
            {
                Id = g.Id,
                Name = g.Name,
                ParentName = g.ParentGroup.Name,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt,
                IsActive = g.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (groups, totalCount);
    }

    public async Task<GroupDto?> GetGroupByIdAsync(Guid id)
    {
        var cacheKey = $"group_{id}";
        
        if (!_cache.TryGetValue(cacheKey, out GroupDto? group))
        {
            group = await _context.Groups.AsNoTracking()
                .Where(g => g.Id == id)
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    ParentGroupId = g.ParentGroupId,
                    Name = g.Name,
                    ParentName = g.ParentGroup.Name,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt,
                    CreatedBy = g.CreatedBy,
                    UpdatedBy = g.UpdatedBy,
                    IsActive = g.RecordState == RecordState.Active
                })
                .FirstOrDefaultAsync();

            if (group != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                
                _cache.Set(cacheKey, group, cacheEntryOptions);
            }
        }

        return group;
    }

    public async Task<Guid> CreateGroupAsync(CreateGroupDto createDto)
    {
        var group = new Group
        {
            ParentGroupId = createDto.ParentGroupId,
            Name = createDto.Name, 
            CreatedBy = createDto.CreatedBy
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return group.Id;
    }

    public async Task<bool> UpdateGroupAsync(Guid id, UpdateGroupDto updateDto)
    {
        var group = await _context.Groups.FindAsync(id);
        
        if (group == null)
            return false;

        group.ParentGroupId = updateDto.ParentGroupId;
        group.Name = updateDto.Name; 
        group.UpdatedBy = updateDto.UpdatedBy;
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _cache.Remove($"group_{id}");
        
        return true;
    }

    public async Task<bool> DeleteGroupAsync(Guid id)
    {
        var group = await _context.Groups.FindAsync(id);
        
        if (group == null)
            return false;

        group.RecordState = RecordState.Deleted;
        group.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _cache.Remove($"group_{id}");
        
        return true;
    }
}