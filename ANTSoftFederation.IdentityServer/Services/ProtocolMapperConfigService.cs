 
namespace ANTSoftFederation.IdentityServer.Services;

public class ProtocolMapperConfigService : BaseService, IProtocolMapperConfigService
{
    private readonly ANTSoftIdentityDbContext _context;

    public ProtocolMapperConfigService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ProtocolMapperConfigListDto> ProtocolMapperConfigs, int TotalCount)> GetProtocolMapperConfigsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ProtocolMapperConfigs.AsQueryable();
         
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(pmc => pmc.Key.Contains(search) || pmc.Value.Contains(search));
        }
         
        var totalCount = await query.CountAsync();

        // Apply pagination
        var protocolMapperConfigs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pmc => new ProtocolMapperConfigListDto
            {
                Id = pmc.Id,
                ConfigKey = pmc.Key,
                ConfigValue = pmc.Value,
                CreatedAt = pmc.CreatedAt,
                UpdatedAt = pmc.UpdatedAt,
                IsActive = pmc.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (protocolMapperConfigs, totalCount);
    }

    public async Task<ProtocolMapperConfigDto?> GetProtocolMapperConfigByIdAsync(Guid id)
    {
        var protocolMapperConfig = await _context.ProtocolMapperConfigs
            .Where(pmc => pmc.Id == id)
            .Select(pmc => new ProtocolMapperConfigDto
            {
                Id = pmc.Id,
                ProtocolMapperId = pmc.ProtocolMapperId,
                ConfigKey = pmc.Key,  // This is correct
                ConfigValue = pmc.Value,  // This is correct
                CreatedAt = pmc.CreatedAt,
                UpdatedAt = pmc.UpdatedAt,
                CreatedBy = pmc.CreatedBy,
                UpdatedBy = pmc.UpdatedBy,
                IsActive = pmc.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return protocolMapperConfig;
    }

    public async Task<Guid> CreateProtocolMapperConfigAsync(CreateProtocolMapperConfigDto createDto)
    {
        var protocolMapperConfig = new ProtocolMapperConfig
        {
            ProtocolMapperId = createDto.ProtocolMapperId,
            Key = createDto.ConfigKey,
            Value = createDto.ConfigValue,
            CreatedBy = createDto.CreatedBy
        };

        _context.ProtocolMapperConfigs.Add(protocolMapperConfig);
        await _context.SaveChangesAsync();

        return protocolMapperConfig.Id;
    }

    public async Task<bool> UpdateProtocolMapperConfigAsync(Guid id, UpdateProtocolMapperConfigDto updateDto)
    {
        var protocolMapperConfig = await _context.ProtocolMapperConfigs.FindAsync(id);
        
        if (protocolMapperConfig == null)
            return false;

        protocolMapperConfig.ProtocolMapperId = updateDto.ProtocolMapperId;
        protocolMapperConfig.Key = updateDto.ConfigKey;
        protocolMapperConfig.Value = updateDto.ConfigValue;
        protocolMapperConfig.UpdatedBy = updateDto.UpdatedBy;
        protocolMapperConfig.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProtocolMapperConfigAsync(Guid id)
    {
        var protocolMapperConfig = await _context.ProtocolMapperConfigs.FindAsync(id);
        
        if (protocolMapperConfig == null)
            return false;

        protocolMapperConfig.RecordState = RecordState.Deleted;
        protocolMapperConfig.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}