
namespace ANTSoftFederation.IdentityServer.DataDbContext;

public class ANTSoftIdentityDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEncryptionProvider _provider;
    public string _userRole = string.Empty;
    public Guid _currentUserId;
    public Guid CompanyId { get; set; }
    public string _applicationKey { get; set; } = string.Empty;
    public string CurrentUserEmail { get; set; } = string.Empty;

    public ANTSoftIdentityDbContext(DbContextOptions<ANTSoftIdentityDbContext> options, IHttpContextAccessor httpAccessor)
        : base(options)
    {
        _httpContextAccessor = httpAccessor ?? throw new ArgumentNullException(nameof(httpAccessor));
        _provider = new GenerateEncryptionProvider("E546C8DF278CD5931069B522E695D4F2");

        if (_httpContextAccessor.HttpContext != null)
        {
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-API-KEY", out var appKey))
            {
                if (!string.IsNullOrWhiteSpace(appKey))
                {
                    ApplicationKey = appKey;
                }
            }

            if (!_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("authorization", out var authorizationHeader))
            {
                return;
            }

            var accessToken = authorizationHeader.LastOrDefault().Split(" ");
            var token = accessToken[1];
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwt = handler.ReadJwtToken(token);
                var currenrUserEmail = jwt.Payload.First(x => x.Key == "unique_name").Value?.ToString();
                var currenrUserId = jwt.Payload.First(x => x.Key == "userId").Value?.ToString();

                if (!string.IsNullOrWhiteSpace(currenrUserId))
                {
                    var userId = Guid.Parse(currenrUserId);
                    CurrentUserId = userId;
                }
            }
        }
    }

    public Guid CurrentUserId
    {
        get => _currentUserId;
        private set => _currentUserId = value;
    }

    public string ApplicationKey
    {
        get => _applicationKey;
        private set => _applicationKey = value;
    }

    public string UserRole
    {
        get => _userRole;
        private set => _userRole = value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=IdentityDb2025;Username=postgres;Password=hVi7P&KaSX@20!24Hr;");
        }
    }

    #region Entities

    public DbSet<Project> Projects { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientScope> ClientScopes { get; set; }
    public DbSet<ClientRole> ClientRoles { get; set; }
    public DbSet<ClientSession> ClientSessions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserProject> UserProjects { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserAttribute> UserAttributes { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<CompositeRole> CompositeRoles { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupPermission> GroupPermissions { get; set; }
    public DbSet<ProjectGroup> ProjectGroups { get; set; }
    
    public DbSet<GroupRole> GroupRoles { get; set; }
    public DbSet<GroupAttribute> GroupAttributes { get; set; }
    public DbSet<ProtocolMapper> ProtocolMappers { get; set; }
    public DbSet<ProtocolMapperConfig> ProtocolMapperConfigs { get; set; }
    public DbSet<IdentityProvider> IdentityProviders { get; set; }
    public DbSet<IdentityProviderConfig> IdentityProviderConfigs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    public DbSet<UserPermission> UserPermissions { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        modelBuilder.EnableAutoHistory<AutoHistory>(options =>
        {
            options.RowIdMaxLength = 128;
            options.ChangedMaxLength = 8096 * 2;
        });

        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(ButterflyIdentityDbContext).Assembly);

        modelBuilder.UseEncryption(_provider);
    }

    public override int SaveChanges()
    {
        this.EnsureAutoHistory();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AuditEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AuditEntities()
    {
        DateTime now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityBase>()
            .Where(p => p.State == EntityState.Added ||
                        p.State == EntityState.Deleted ||
                        p.State == EntityState.Modified).ToArray())
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = now;
                entity.CreatedBy = CurrentUserEmail;
                entity.RecordState = RecordState.Active;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = now;
                entity.UpdatedBy = CurrentUserEmail;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entity.UpdatedAt = now;
                entity.RecordState = RecordState.Deleted;
                entity.UpdatedBy = CurrentUserEmail;
            }
        }
    }
}
