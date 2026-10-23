
namespace SorteFederation.ANTApplications.DataDbContext;

public partial class ANTSoftPortalDbContext : DbContext
{
    private readonly DatabaseService _databaseService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IEncryptionProvider _provider;
     
    public string _applicationKey { get; set; } = string.Empty;
     
    public ANTSoftPortalDbContext(
        DbContextOptions<ANTSoftPortalDbContext> options,
        DatabaseService databaseService,
        IHttpContextAccessor httpAccessor
        )
        : base(options)
    {
        _databaseService = databaseService;
        _httpContextAccessor = httpAccessor;

        _provider = new GenerateEncryptionProvider("E546C8DF278CD5931069B522E695D4F2");
    }

    public bool IsSuperAdminTenant => CurrentUser is not null && RoleNames.IsSuperAdmin(CurrentUser);

    public Guid? CurrentCompanyId => Guid.TryParse(CurrentUser?.FindFirstValue("company_id"), out var companyId) ? companyId : null;
    private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;
    private Guid CurrentCompanyIdForTenantFilter => CurrentCompanyId ?? Guid.Empty;
    private bool ShouldApplyCompanyTenantFilter => CurrentUser?.Identity?.IsAuthenticated == true && !IsSuperAdminTenant;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _databaseService.ConfigureDatabase(optionsBuilder);

    }

    public DbSet<User> Users => Set<User>();
    public virtual DbSet<ProjectApplication> ProjectApplications => Set<ProjectApplication>();
    public virtual DbSet<ApplicationIntegratorKey> ApplicationIntegratorKeys => Set<ApplicationIntegratorKey>();
    public virtual DbSet<ApplicationEmbedding> ApplicationEmbeddings => Set<ApplicationEmbedding>();
    public virtual DbSet<ApplicationModuleIssueEmbedding> ApplicationModuleIssueEmbeddings => Set<ApplicationModuleIssueEmbedding>();
    public virtual DbSet<ApplicationDoc> ApplicationDocs => Set<ApplicationDoc>();
    public virtual DbSet<ApplicationClaim> ApplicationClaims => Set<ApplicationClaim>();
    public virtual DbSet<ApplicationModuleIssue> ApplicationModuleIssues => Set<ApplicationModuleIssue>();
    public virtual DbSet<ApplicationModule> ApplicationModules => Set<ApplicationModule>();
    public virtual DbSet<ApplicationMenu> ApplicationMenus => Set<ApplicationMenu>();
    public virtual DbSet<Company> Companies => Set<Company>();
    public virtual DbSet<CompanyAppVersion> CompanyAppVersions => Set<CompanyAppVersion>();
    public virtual DbSet<CompanyApplication> CompanyApplications => Set<CompanyApplication>();
    public virtual DbSet<CompanyApplicationLicence> CompanyApplicationLicences => Set<CompanyApplicationLicence>();
    public virtual DbSet<ApplicationModuleIssueMessage> ApplicationModuleIssueMessages => Set<ApplicationModuleIssueMessage>();
    public virtual DbSet<UserApplicationChat> UserApplicationChats => Set<UserApplicationChat>();

    public virtual DbSet<PortalTenant> PortalTenants => Set<PortalTenant>();
    public virtual DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public virtual DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public virtual DbSet<TenantSubscriptionPayment> TenantSubscriptionPayments => Set<TenantSubscriptionPayment>();

    public virtual DbSet<Country> Countries => Set<Country>();
    public virtual DbSet<CurrencyType> CurrencyTypes => Set<CurrencyType>();
    public virtual DbSet<DailyExchangeRate> DailyExchangeRates => Set<DailyExchangeRate>();

    public virtual DbSet<Group> Groups => Set<Group>();
    public virtual DbSet<GroupPermission> GroupPermissions => Set<GroupPermission>();
    public virtual DbSet<GroupRole> GroupRoles => Set<GroupRole>();
    public virtual DbSet<Permission> Permissions => Set<Permission>();
    public virtual DbSet<Role> Roles => Set<Role>();
    public virtual DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public virtual DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public virtual DbSet<UserRole> UserRoles => Set<UserRole>();
    public virtual DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public virtual DbSet<UserSession> UserSessions => Set<UserSession>();
    public virtual DbSet<UserDataGridProperty> UserDataGridProperties => Set<UserDataGridProperty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ANTSoftPortalDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");

        var provider = new GenerateEncryptionProvider("Z546B8DF278CD5931069B522E695D4E8");

        modelBuilder.UseEncryption(provider);

    }

    public override int SaveChanges()
    {
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
        var user = CurrentUser;
        var actor = user?.FindFirstValue(ClaimTypes.Name)
                    ?? user?.FindFirstValue("preferred_username")
                    ?? user?.FindFirstValue("unique_name")
                    ?? user?.Identity?.Name
                    ?? "system";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>()
            .Where(p => p.State == EntityState.Added ||
                   p.State == EntityState.Deleted ||
                   p.State == EntityState.Modified).ToArray()
                   )
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.Id = Guid.CreateVersion7();
                entity.CreatedAt = now;
                entity.CreatedBy = actor;
                entity.RecordState = RecordState.Active;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = now;
                entity.UpdatedBy = actor;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entity.UpdatedAt = now;
                entity.RecordState = RecordState.Deleted;
                entity.UpdatedBy = actor;
            }
        }
    }

}
