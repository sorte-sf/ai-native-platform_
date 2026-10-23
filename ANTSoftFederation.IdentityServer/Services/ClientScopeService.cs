
namespace ANTSoftFederation.IdentityServer.Services;

public class ClientScopeService : BaseService, IClientScopeService
{
    private readonly ANTSoftIdentityDbContext _context;

    public ClientScopeService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ClientScopeListDto> ClientScopes, int TotalCount)> GetClientScopesAsync(int page, int pageSize, string? search)
    {
        var query = _context.ClientScopes.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(cs => cs.Name.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var clientScopes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cs => new ClientScopeListDto
            {
                Id = cs.Id,
                Name = cs.Name,
                Description = cs.Description,
                CreatedAt = cs.CreatedAt,
                UpdatedAt = cs.UpdatedAt,
                IsActive = cs.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (ClientScopes: clientScopes, TotalCount: totalCount);
    }

    public async Task<ClientScopeDto?> GetClientScopeByIdAsync(Guid id)
    {
        var clientScope = await _context.ClientScopes
            .Where(cs => cs.Id == id)
            .Select(cs => new ClientScopeDto
            {
                Id = cs.Id,
                ProjectId = cs.ProjectId,
                Name = cs.Name,
                Description = cs.Description,
                Type = cs.Type,
                CreatedAt = cs.CreatedAt,
                UpdatedAt = cs.UpdatedAt,
                CreatedBy = cs.CreatedBy,
                UpdatedBy = cs.UpdatedBy,
                IsActive = cs.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return clientScope;
    }

    public async Task<Guid> CreateClientScopeAsync(CreateClientScopeDto createDto)
    {
        var clientScope = new ClientScope
        {
            ProjectId = createDto.ProjectId,
            Name = createDto.Name,
            Description = createDto.Description,
            Type = createDto.Type,
            CreatedBy = createDto.CreatedBy
        };

        _context.ClientScopes.Add(clientScope);
        await _context.SaveChangesAsync();

        return clientScope.Id;
    }

    public async Task<bool> UpdateClientScopeAsync(Guid id, UpdateClientScopeDto updateDto)
    {
        var clientScope = await _context.ClientScopes.FindAsync(id);
        
        if (clientScope == null)
            return false;

        clientScope.ProjectId = updateDto.ProjectId;
        clientScope.Name = updateDto.Name;
        clientScope.Description = updateDto.Description;
        clientScope.Type = updateDto.Type;
        clientScope.UpdatedBy = updateDto.UpdatedBy;
        clientScope.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteClientScopeAsync(Guid id)
    {
        var clientScope = await _context.ClientScopes.FindAsync(id);
        
        if (clientScope == null)
            return false;

        clientScope.RecordState = RecordState.Deleted;
        clientScope.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}