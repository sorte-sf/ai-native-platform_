
namespace ANTSoftFederation.IdentityServer.Services;

public class IdentityProviderService : BaseService, IIdentityProviderService
{
    private readonly ANTSoftIdentityDbContext _context;

    public IdentityProviderService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<IdentityProviderListDto> IdentityProviders, int TotalCount)> GetIdentityProvidersAsync(int page, int pageSize, string? search)
    {
        var query = _context.IdentityProviders.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(ip => ip.Alias.Contains(search) || ip.ProviderId.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var identityProviders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ip => new IdentityProviderListDto
            {
                Id = ip.Id,
                Alias = ip.Alias,
                DisplayName = ip.DisplayName,
                ProviderId = ip.ProviderId,
                Enabled = ip.Enabled,
                CreatedAt = ip.CreatedAt,
                UpdatedAt = ip.UpdatedAt,
                IsActive = ip.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (IdentityProviders: identityProviders, TotalCount: totalCount);
    }

    public async Task<IdentityProviderDto?> GetIdentityProviderByIdAsync(Guid id)
    {
        var identityProvider = await _context.IdentityProviders
            .Where(ip => ip.Id == id)
            .Select(ip => new IdentityProviderDto
            {
                Id = ip.Id,
                RealmId = ip.RealmId,
                Alias = ip.Alias,
                DisplayName = ip.DisplayName,
                ProviderId = ip.ProviderId,
                Enabled = ip.Enabled,
                TrustEmail = ip.TrustEmail,
                StoreToken = ip.StoreToken,
                AddReadTokenRoleOnCreate = ip.AddReadTokenRoleOnCreate,
                LinkOnly = ip.LinkOnly,
                FirstBrokerLoginFlowAlias = ip.FirstBrokerLoginFlowAlias,
                CreatedAt = ip.CreatedAt,
                UpdatedAt = ip.UpdatedAt,
                CreatedBy = ip.CreatedBy,
                UpdatedBy = ip.UpdatedBy,
                IsActive = ip.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return identityProvider;
    }

    public async Task<Guid> CreateIdentityProviderAsync(CreateIdentityProviderDto createDto)
    {
        var identityProvider = new IdentityProvider
        {
            RealmId = createDto.RealmId,
            Alias = createDto.Alias,
            DisplayName = createDto.DisplayName,
            ProviderId = createDto.ProviderId,
            Enabled = createDto.Enabled,
            TrustEmail = createDto.TrustEmail,
            StoreToken = createDto.StoreToken,
            AddReadTokenRoleOnCreate = createDto.AddReadTokenRoleOnCreate,
            LinkOnly = createDto.LinkOnly,
            FirstBrokerLoginFlowAlias = createDto.FirstBrokerLoginFlowAlias,
            CreatedBy = createDto.CreatedBy
        };

        _context.IdentityProviders.Add(identityProvider);
        await _context.SaveChangesAsync();

        return identityProvider.Id;
    }

    public async Task<bool> UpdateIdentityProviderAsync(Guid id, UpdateIdentityProviderDto updateDto)
    {
        var identityProvider = await _context.IdentityProviders.FindAsync(id);
        
        if (identityProvider == null)
            return false;

        identityProvider.RealmId = updateDto.RealmId;
        identityProvider.Alias = updateDto.Alias;
        identityProvider.DisplayName = updateDto.DisplayName;
        identityProvider.ProviderId = updateDto.ProviderId;
        identityProvider.Enabled = updateDto.Enabled;
        identityProvider.TrustEmail = updateDto.TrustEmail;
        identityProvider.StoreToken = updateDto.StoreToken;
        identityProvider.AddReadTokenRoleOnCreate = updateDto.AddReadTokenRoleOnCreate;
        identityProvider.LinkOnly = updateDto.LinkOnly;
        identityProvider.FirstBrokerLoginFlowAlias = updateDto.FirstBrokerLoginFlowAlias;
        identityProvider.UpdatedBy = updateDto.UpdatedBy;
        identityProvider.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteIdentityProviderAsync(Guid id)
    {
        var identityProvider = await _context.IdentityProviders.FindAsync(id);
        
        if (identityProvider == null)
            return false;

        identityProvider.RecordState = RecordState.Deleted;
        identityProvider.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}