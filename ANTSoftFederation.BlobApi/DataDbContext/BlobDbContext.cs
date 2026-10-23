
using SorteFederation.EncryptEFCoreColumn;

namespace SorteFederation.BlobApi.DataDbContext;

public partial class BlobDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IEncryptionProvider _provider;

    public string _userRole;

    public string _currentUserEmail;

    public Guid _currentUserId;

    public string _applicationKey { get; set; } = string.Empty;

    // public BlobDbContext() { }

    public BlobDbContext(
        DbContextOptions<BlobDbContext> options,
        IHttpContextAccessor httpAccessor
        )
        : base(options)
    {
        _httpContextAccessor = httpAccessor;

        if (_httpContextAccessor?.HttpContext != null)
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

                var currenrUserId = jwt.Payload.First(x => x.Key == "userId").Value?.ToString();

                var CurrentUserEmail = jwt.Payload.First(x => x.Key == "unique_name").Value?.ToString();

                if (!string.IsNullOrWhiteSpace(currenrUserId))
                {
                    var userId = Guid.Parse(currenrUserId);
                    CurrentUserId = userId;
                }

            }

        }

        _provider = new GenerateEncryptionProvider("E546C8DF278CD5931069B522E695D4F2");

    }

    public Guid CurrentUserId
    {
        get
        {
            return _currentUserId;
        }

        private set
        {
            _currentUserId = value;
        }
    }

    public string ApplicationKey
    {
        get
        {
            return _applicationKey;
        }

        private set
        {
            _applicationKey = value;
        }
    }
    public string UserRoleId
    {
        get
        {
            return _userRole;
        }

        private set
        {
            _userRole = value;
        }
    }

    public string CurrentUserEmail
    {
        get
        {
            return _currentUserEmail;
        }

        private set
        {
            _currentUserEmail = value;
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
     
    public virtual DbSet<Document> Documents => Set<Document>();
    public virtual DbSet<DocumentAccessControl> DocumentAccessControls => Set<DocumentAccessControl>();
    public virtual DbSet<DocumentCategory> DocumentCategoreis => Set<DocumentCategory>(); 
    public virtual DbSet<DocumentMetadataValue> DocumentMetadataValues => Set<DocumentMetadataValue>();
    public virtual DbSet<DocumentTag> DocumentTags => Set<DocumentTag>();
    public virtual DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();
    public virtual DbSet<DocumentTemplateMetadataField> DocumentTemplateMetadataFields => Set<DocumentTemplateMetadataField>();
    public virtual DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public virtual DbSet<DocumentVersionApproval> DocumentVersionApprovals => Set<DocumentVersionApproval>();
    public virtual DbSet<DocumentWorkflow> DocumentWorkflows => Set<DocumentWorkflow>();
    public virtual DbSet<DocumentWorkflowInstance> DocumentWorkflowInstances => Set<DocumentWorkflowInstance>();
    public virtual DbSet<DocumentWorkflowStep> DocumentWorkflowSteps => Set<DocumentWorkflowStep>();
    public virtual DbSet<DocumentWorkflowStepApproval> DocumentWorkflowStepApprovals => Set<DocumentWorkflowStepApproval>();
    public virtual DbSet<DocumentWorkflowStepInstance> DocumentWorkflowStepInstances => Set<DocumentWorkflowStepInstance>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.EnableAutoHistory<AutoHistory>((options) =>
        {
            options.RowIdMaxLength = 128;
            options.ChangedMaxLength = 8096 * 2;
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlobDbContext).Assembly);

        var provider = new GenerateEncryptionProvider("Z546B8DF278CD5931069B522E695D4E8");

        modelBuilder.UseEncryption(provider);

    }

    public override int SaveChanges()
    {
        this.EnsureAutoHistory();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.EnsureAutoHistory();
        AuditEntities();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AuditEntities()
    {
        DateTime now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>()
            .Where(p => p.State == EntityState.Added ||
                   p.State == EntityState.Deleted ||
                   p.State == EntityState.Modified).ToArray()
                   )
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.Id = Guid.NewGuid();
                entity.CreatedDate= now;
                entity.IsDeleted = false;
                entity.CreatedBy = CurrentUserEmail;
                //entity.RecordState = RecordState.Active;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.ModifiedDate = now;
                entity.ModifiedBy   = CurrentUserEmail;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entity.ModifiedDate  = now;
                entity.IsDeleted = true;
                entity.ModifiedBy= CurrentUserEmail;
            }
        }
    }

}
 