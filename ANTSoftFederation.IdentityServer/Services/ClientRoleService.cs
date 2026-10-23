 
namespace ANTSoftFederation.IdentityServer.Services;

public class ClientRoleService : BaseService, IClientRoleService
{
    private readonly ANTSoftIdentityDbContext _context;

    public ClientRoleService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ClientRoleListDto> ClientRoles, int TotalCount)> GetClientRolesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ClientRoles.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            // ClientRole has no searchable fields, so we'll skip search filter
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var clientRoles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cr => new ClientRoleListDto
            {
                Id = cr.Id,
                ClientId = cr.ClientId,
                RoleId = cr.RoleId,
                CreatedAt = cr.CreatedAt,
                UpdatedAt = cr.UpdatedAt,
                IsActive = cr.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (clientRoles, totalCount);
    }

    public async Task<ClientRoleDto?> GetClientRoleByIdAsync(Guid clientId, Guid roleId)
    {
        var clientRole = await _context.ClientRoles
            .Where(cr => cr.ClientId == clientId && cr.RoleId == roleId)
            .Select(cr => new ClientRoleDto
            {
                Id = cr.Id,
                ClientId = cr.ClientId,
                RoleId = cr.RoleId,
                CreatedAt = cr.CreatedAt,
                UpdatedAt = cr.UpdatedAt,
                CreatedBy = cr.CreatedBy,
                UpdatedBy = cr.UpdatedBy,
                IsActive = cr.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return clientRole;
    }

    public async Task<bool> CreateClientRoleAsync(CreateClientRoleDto createDto)
    {
        var clientRole = new ClientRole
        {
            ClientId = createDto.ClientId,
            RoleId = createDto.RoleId,
            CreatedBy = createDto.CreatedBy
        };

        _context.ClientRoles.Add(clientRole);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateClientRoleAsync(Guid clientId, Guid roleId, UpdateClientRoleDto updateDto)
    {
        var clientRole = await _context.ClientRoles
            .FirstOrDefaultAsync(cr => cr.ClientId == clientId && cr.RoleId == roleId);
        
        if (clientRole == null)
            return false;

        clientRole.ClientId = updateDto.ClientId;
        clientRole.RoleId = updateDto.RoleId;
        clientRole.UpdatedBy = updateDto.UpdatedBy;
        clientRole.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteClientRoleAsync(Guid clientId, Guid roleId)
    {
        var clientRole = await _context.ClientRoles
            .FirstOrDefaultAsync(cr => cr.ClientId == clientId && cr.RoleId == roleId);
        
        if (clientRole == null)
            return false;

        clientRole.RecordState = RecordState.Deleted;
        clientRole.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}