
namespace ANTSoftFederation.IdentityServer.Services;

public class ClientService : BaseService, IClientService
{
    private readonly ANTSoftIdentityDbContext _context;

    public ClientService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ClientListDto> Clients, int TotalCount)> GetClientsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.Clients.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.ClientId.Contains(search) || c.Name.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var clients = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClientListDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                Name = c.Name,
                Enabled = c.Enabled,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                IsActive = c.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (clients, totalCount);
    }

    public async Task<ClientDto?> GetClientByIdAsync(Guid id)
    {
        var client = await _context.Clients
            .Where(c => c.Id == id)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                ProjectId = c.ProjectId,
                ClientId = c.ClientId,
                Name = c.Name,
                Description = c.Description,
                Enabled = c.Enabled,
                Protocol = c.Protocol,
                AccessType = c.AccessType,
                ClientSecret = c.ClientSecret,
                ServiceAccountsEnabled = c.ServiceAccountsEnabled,
                AuthorizationServicesEnabled = c.AuthorizationServicesEnabled,
                StandardFlowEnabled = c.StandardFlowEnabled,
                ImplicitFlowEnabled = c.ImplicitFlowEnabled,
                DirectAccessGrantsEnabled = c.DirectAccessGrantsEnabled,
                RootUrl = c.RootUrl,
                AdminUrl = c.AdminUrl,
                BaseUrl = c.BaseUrl,
                RedirectUris = c.RedirectUris,
                WebOrigins = c.WebOrigins,
                AccessTokenLifespan = c.AccessTokenLifespan,
                AlwaysDisplayInConsole = c.AlwaysDisplayInConsole,
                ConsentRequired = c.ConsentRequired,
                DisplayOnConsentScreen = c.DisplayOnConsentScreen,
                ConsentScreenText = c.ConsentScreenText,
                PkceCodeChallengeMethod = c.PkceCodeChallengeMethod,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedBy = c.CreatedBy,
                UpdatedBy = c.UpdatedBy,
                IsActive = c.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return client;
    }

    public async Task<Guid> CreateClientAsync(CreateClientDto createDto)
    {
        var client = new Client
        {
            ProjectId = createDto.ProjectId,
            ClientId = createDto.ClientId,
            Name = createDto.Name,
            Description = createDto.Description,
            Enabled = createDto.Enabled,
            Protocol = createDto.Protocol,
            AccessType = createDto.AccessType,
            ClientSecret = createDto.ClientSecret,
            ServiceAccountsEnabled = createDto.ServiceAccountsEnabled,
            AuthorizationServicesEnabled = createDto.AuthorizationServicesEnabled,
            StandardFlowEnabled = createDto.StandardFlowEnabled,
            ImplicitFlowEnabled = createDto.ImplicitFlowEnabled,
            DirectAccessGrantsEnabled = createDto.DirectAccessGrantsEnabled,
            RootUrl = createDto.RootUrl,
            AdminUrl = createDto.AdminUrl,
            BaseUrl = createDto.BaseUrl,
            RedirectUris = createDto.RedirectUris,
            WebOrigins = createDto.WebOrigins,
            AccessTokenLifespan = createDto.AccessTokenLifespan,
            AlwaysDisplayInConsole = createDto.AlwaysDisplayInConsole,
            ConsentRequired = createDto.ConsentRequired,
            DisplayOnConsentScreen = createDto.DisplayOnConsentScreen,
            ConsentScreenText = createDto.ConsentScreenText,
            PkceCodeChallengeMethod = createDto.PkceCodeChallengeMethod,
            CreatedBy = createDto.CreatedBy
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return client.Id;
    }

    public async Task<bool> UpdateClientAsync(Guid id, UpdateClientDto updateDto)
    {
        var client = await _context.Clients.FindAsync(id);
        
        if (client == null)
            return false;

        client.ProjectId = updateDto.ProjectId;
        client.ClientId = updateDto.ClientId;
        client.Name = updateDto.Name;
        client.Description = updateDto.Description;
        client.Enabled = updateDto.Enabled;
        client.Protocol = updateDto.Protocol;
        client.AccessType = updateDto.AccessType;
        client.ClientSecret = updateDto.ClientSecret;
        client.ServiceAccountsEnabled = updateDto.ServiceAccountsEnabled;
        client.AuthorizationServicesEnabled = updateDto.AuthorizationServicesEnabled;
        client.StandardFlowEnabled = updateDto.StandardFlowEnabled;
        client.ImplicitFlowEnabled = updateDto.ImplicitFlowEnabled;
        client.DirectAccessGrantsEnabled = updateDto.DirectAccessGrantsEnabled;
        client.RootUrl = updateDto.RootUrl;
        client.AdminUrl = updateDto.AdminUrl;
        client.BaseUrl = updateDto.BaseUrl;
        client.RedirectUris = updateDto.RedirectUris;
        client.WebOrigins = updateDto.WebOrigins;
        client.AccessTokenLifespan = updateDto.AccessTokenLifespan;
        client.AlwaysDisplayInConsole = updateDto.AlwaysDisplayInConsole;
        client.ConsentRequired = updateDto.ConsentRequired;
        client.DisplayOnConsentScreen = updateDto.DisplayOnConsentScreen;
        client.ConsentScreenText = updateDto.ConsentScreenText;
        client.PkceCodeChallengeMethod = updateDto.PkceCodeChallengeMethod;
        client.UpdatedBy = updateDto.UpdatedBy;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteClientAsync(Guid id)
    {
        var client = await _context.Clients.FindAsync(id);
        
        if (client == null)
            return false;

        client.RecordState = RecordState.Deleted;
        client.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}