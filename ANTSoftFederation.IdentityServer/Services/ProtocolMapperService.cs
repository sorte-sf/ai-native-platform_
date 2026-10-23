 
namespace ANTSoftFederation.IdentityServer.Services;

public class ProtocolMapperService : BaseService, IProtocolMapperService
{
    private readonly ANTSoftIdentityDbContext _context;

    public ProtocolMapperService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ProtocolMapperListDto> ProtocolMappers, int TotalCount)> GetProtocolMappersAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ProtocolMappers.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(pm => pm.Name.Contains(search) || pm.Protocol.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var protocolMappers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pm => new ProtocolMapperListDto
            {
                Id = pm.Id,
                Name = pm.Name,
                Protocol = pm.Protocol,
                CreatedAt = pm.CreatedAt,
                UpdatedAt = pm.UpdatedAt,
                IsActive = pm.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (protocolMappers, totalCount);
    }

    public async Task<ProtocolMapperDto?> GetProtocolMapperByIdAsync(Guid id)
    {
        var protocolMapper = await _context.ProtocolMappers
            .Where(pm => pm.Id == id)
            .Select(pm => new ProtocolMapperDto
            {
                Id = pm.Id,
                ClientId = pm.ClientId,
                Name = pm.Name, 
                Protocol = pm.Protocol,
                MapperType = pm.ProtocolMapperType,
                CreatedAt = pm.CreatedAt,
                UpdatedAt = pm.UpdatedAt,
                CreatedBy = pm.CreatedBy,
                UpdatedBy = pm.UpdatedBy,
                IsActive = pm.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return protocolMapper;
    }

    public async Task<Guid> CreateProtocolMapperAsync(CreateProtocolMapperDto createDto)
    {
        var protocolMapper = new ProtocolMapper
        {
            ClientId = createDto.ClientId,
            Name = createDto.Name, 
            Protocol = createDto.Protocol,
            ProtocolMapperType = createDto.MapperType
        };

        _context.ProtocolMappers.Add(protocolMapper);
        await _context.SaveChangesAsync();

        return protocolMapper.Id;
    }

    public async Task<bool> UpdateProtocolMapperAsync(Guid id, UpdateProtocolMapperDto updateDto)
    {
        var protocolMapper = await _context.ProtocolMappers.FindAsync(id);
        
        if (protocolMapper == null)
            return false;

        protocolMapper.ClientId = updateDto.ClientId;
        protocolMapper.Name = updateDto.Name; 
        protocolMapper.Protocol = updateDto.Protocol;
        protocolMapper.ProtocolMapperType = updateDto.MapperType;
        protocolMapper.UpdatedBy = updateDto.UpdatedBy;
        protocolMapper.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProtocolMapperAsync(Guid id)
    {
        var protocolMapper = await _context.ProtocolMappers.FindAsync(id);
        
        if (protocolMapper == null)
            return false;

        protocolMapper.RecordState = RecordState.Deleted;
        protocolMapper.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}