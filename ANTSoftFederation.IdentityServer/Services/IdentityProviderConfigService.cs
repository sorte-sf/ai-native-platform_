 
namespace ANTSoftFederation.IdentityServer.Services;

public class IdentityProviderConfigService : BaseService, IIdentityProviderConfigService
{
    private readonly ANTSoftIdentityDbContext _context;

    public IdentityProviderConfigService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<IdentityProviderConfigListDto> IdentityProviderConfigs, int TotalCount)> GetIdentityProviderConfigsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.IdentityProviderConfigs.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(ipc => ipc.Key.Contains(search) || ipc.Value.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var identityProviderConfigs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ipc => new IdentityProviderConfigListDto
            {
                Id = ipc.Id,
                ConfigKey = ipc.Key,
                ConfigValue = ipc.Value,
                CreatedAt = ipc.CreatedAt,
                UpdatedAt = ipc.UpdatedAt,
                IsActive = ipc.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (identityProviderConfigs, totalCount);
    }

    public async Task<IdentityProviderConfigDto?> GetIdentityProviderConfigByIdAsync(Guid id)
    {
        var identityProviderConfig = await _context.IdentityProviderConfigs
            .Where(ipc => ipc.Id == id)
            .Select(ipc => new IdentityProviderConfigDto
            {
                Id = ipc.Id,
                IdentityProviderId = ipc.IdentityProviderId,
                ConfigKey = ipc.Key,
                ConfigValue = ipc.Value,
                CreatedAt = ipc.CreatedAt,
                UpdatedAt = ipc.UpdatedAt,
                CreatedBy = ipc.CreatedBy,
                UpdatedBy = ipc.UpdatedBy,
                IsActive = ipc.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return identityProviderConfig;
    }

    public async Task<Guid> CreateIdentityProviderConfigAsync(CreateIdentityProviderConfigDto createDto)
    {
        var identityProviderConfig = new IdentityProviderConfig
        {
            IdentityProviderId = createDto.IdentityProviderId,
            Key = createDto.ConfigKey,
            Value = createDto.ConfigValue,
            CreatedBy = createDto.CreatedBy
        };

        _context.IdentityProviderConfigs.Add(identityProviderConfig);
        await _context.SaveChangesAsync();

        return identityProviderConfig.Id;
    }

    public async Task<bool> UpdateIdentityProviderConfigAsync(Guid id, UpdateIdentityProviderConfigDto updateDto)
    {
        var identityProviderConfig = await _context.IdentityProviderConfigs.FindAsync(id);
        
        if (identityProviderConfig == null)
            return false;

        identityProviderConfig.IdentityProviderId = updateDto.IdentityProviderId;
        identityProviderConfig.Key = updateDto.ConfigKey;
        identityProviderConfig.Value = updateDto.ConfigValue;
        identityProviderConfig.UpdatedBy = updateDto.UpdatedBy;
        identityProviderConfig.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteIdentityProviderConfigAsync(Guid id)
    {
        var identityProviderConfig = await _context.IdentityProviderConfigs.FindAsync(id);
        
        if (identityProviderConfig == null)
            return false;

        identityProviderConfig.RecordState = RecordState.Deleted;
        identityProviderConfig.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}