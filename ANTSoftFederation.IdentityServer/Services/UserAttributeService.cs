
namespace ANTSoftFederation.IdentityServer.Services;

public class UserAttributeService : BaseService, IUserAttributeService
{
    private readonly ANTSoftIdentityDbContext _context;

    public UserAttributeService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<UserAttributeListDto> UserAttributes, int TotalCount)> GetUserAttributesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.UserAttributes.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(ua => ua.Name.Contains(search) || ua.Value.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var userAttributes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ua => new UserAttributeListDto
            {
                Id = Guid.NewGuid(), // UserAttribute doesn't have an Id property
                Name = ua.Name,
                Value = ua.Value,
                CreatedAt = ua.CreatedAt,
                UpdatedAt = ua.UpdatedAt,
                IsActive = ua.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (userAttributes, totalCount);
    }

    public async Task<UserAttributeDto?> GetUserAttributeByIdAsync(Guid userId, string name)
    {
        var userAttribute = await _context.UserAttributes
            .Where(ua => ua.UserId == userId && ua.Name == name)
            .Select(ua => new UserAttributeDto
            {
                UserId = ua.UserId,
                Name = ua.Name,
                Value = ua.Value,
                CreatedAt = ua.CreatedAt,
                UpdatedAt = ua.UpdatedAt,
                CreatedBy = ua.CreatedBy,
                UpdatedBy = ua.UpdatedBy,
                IsActive = ua.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return userAttribute;
    }

    public async Task<bool> CreateUserAttributeAsync(CreateUserAttributeDto createDto)
    {
        var userAttribute = new UserAttribute
        {
            UserId = createDto.UserId,
            Name = createDto.Name,
            Value = createDto.Value,
            CreatedBy = createDto.CreatedBy
        };

        _context.UserAttributes.Add(userAttribute);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateUserAttributeAsync(Guid userId, string name, UpdateUserAttributeDto updateDto)
    {
        var userAttribute = await _context.UserAttributes
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.Name == name);
        
        if (userAttribute == null)
            return false;

        userAttribute.UserId = updateDto.UserId;
        userAttribute.Name = updateDto.Name;
        userAttribute.Value = updateDto.Value;
        userAttribute.UpdatedBy = updateDto.UpdatedBy;
        userAttribute.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAttributeAsync(Guid userId, string name)
    {
        var userAttribute = await _context.UserAttributes
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.Name == name);
        
        if (userAttribute == null)
            return false;

        userAttribute.RecordState = RecordState.Deleted;
        userAttribute.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}