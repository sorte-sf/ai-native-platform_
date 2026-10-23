 
namespace ANTSoftFederation.IdentityServer.Services;

public class GroupAttributeService : BaseService, IGroupAttributeService
{
    private readonly ANTSoftIdentityDbContext _context;

    public GroupAttributeService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<GroupAttributeListDto> GroupAttributes, int TotalCount)> GetGroupAttributesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.GroupAttributes.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(ga => ga.Name.Contains(search) || ga.Value.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var groupAttributes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ga => new GroupAttributeListDto
            {
                Id = ga.Id,
                Name = ga.Name,
                Value = ga.Value,
                CreatedAt = ga.CreatedAt,
                UpdatedAt = ga.UpdatedAt,
                IsActive = ga.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (groupAttributes, totalCount);
    }

    public async Task<GroupAttributeDto?> GetGroupAttributeByIdAsync(Guid id)
    {
        var groupAttribute = await _context.GroupAttributes
            .Where(ga => ga.Id == id)
            .Select(ga => new GroupAttributeDto
            {
                Id = ga.Id,
                GroupId = ga.GroupId,
                Name = ga.Name,
                Value = ga.Value,
                CreatedAt = ga.CreatedAt,
                UpdatedAt = ga.UpdatedAt,
                CreatedBy = ga.CreatedBy,
                UpdatedBy = ga.UpdatedBy,
                IsActive = ga.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return groupAttribute;
    }

    public async Task<Guid> CreateGroupAttributeAsync(CreateGroupAttributeDto createDto)
    {
        var groupAttribute = new GroupAttribute
        {
            GroupId = createDto.GroupId,
            Name = createDto.Name,
            Value = createDto.Value,
            CreatedBy = createDto.CreatedBy
        };

        _context.GroupAttributes.Add(groupAttribute);
        await _context.SaveChangesAsync();

        return groupAttribute.Id;
    }

    public async Task<bool> UpdateGroupAttributeAsync(Guid id, UpdateGroupAttributeDto updateDto)
    {
        var groupAttribute = await _context.GroupAttributes.FindAsync(id);
        
        if (groupAttribute == null)
            return false;

        groupAttribute.GroupId = updateDto.GroupId;
        groupAttribute.Name = updateDto.Name;
        groupAttribute.Value = updateDto.Value;
        groupAttribute.UpdatedBy = updateDto.UpdatedBy;
        groupAttribute.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteGroupAttributeAsync(Guid id)
    {
        var groupAttribute = await _context.GroupAttributes.FindAsync(id);
        
        if (groupAttribute == null)
            return false;

        groupAttribute.RecordState = RecordState.Deleted;
        groupAttribute.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}