
namespace SorteFederation.Builders.Data;

public sealed class TenantRuntimeDbContext(DbContextOptions<TenantRuntimeDbContext> options, Guid tenantId = default) : DbContext(options)
{
    public Guid TenantId { get; } = tenantId;
    public DbSet<BuilderRecord> Records => Set<BuilderRecord>();
    public DbSet<BuilderRecordRelationship> RecordRelationships => Set<BuilderRecordRelationship>();
    public DbSet<BuilderWorkflowInstance> WorkflowInstances => Set<BuilderWorkflowInstance>();
    public DbSet<BuilderWorkflowTask> WorkflowTasks => Set<BuilderWorkflowTask>();
    public DbSet<BuilderKnowledgeDocument> KnowledgeDocuments => Set<BuilderKnowledgeDocument>();
    public DbSet<BuilderKnowledgeChunk> KnowledgeChunks => Set<BuilderKnowledgeChunk>();
    public DbSet<BuilderSupportTicket> SupportTickets => Set<BuilderSupportTicket>();
    public DbSet<BuilderSupportComment> SupportComments => Set<BuilderSupportComment>();
    protected override void OnModelCreating(ModelBuilder m) {
        m.HasDefaultSchema("builders_runtime");
        m.HasPostgresExtension("vector");
        m.Entity<BuilderRecord>().HasQueryFilter(x => x.TenantId == TenantId);
        m.Entity<BuilderRecordRelationship>().HasQueryFilter(x => x.TenantId == TenantId);
        m.Entity<BuilderWorkflowInstance>().HasQueryFilter(x => x.TenantId == TenantId);
        m.Entity<BuilderWorkflowTask>().HasQueryFilter(x => x.TenantId == TenantId);
        m.Entity<BuilderKnowledgeDocument>().HasQueryFilter(x => x.TenantId == TenantId);
        m.Entity<BuilderKnowledgeChunk>().HasQueryFilter(x => x.TenantId == TenantId);
        m.Entity<BuilderSupportTicket>().HasQueryFilter(x => x.TenantId == TenantId);
        m.Entity<BuilderSupportComment>().HasQueryFilter(x => x.TenantId == TenantId);
        m.Entity<BuilderRecord>().Property(x=>x.DataJson).HasColumnType("jsonb"); 
        m.Entity<BuilderRecord>().HasIndex(x => new { x.TenantId, x.ObjectId, x.UpdatedAt });
        m.Entity<BuilderRecordRelationship>().HasIndex(x=>new{x.TenantId,x.RelationshipId,x.SourceRecordId,x.TargetRecordId}).IsUnique(); m.Entity<BuilderWorkflowInstance>().HasIndex(x=>new{x.TenantId,x.WorkflowId,x.RecordId}).IsUnique();
        m.Entity<BuilderWorkflowInstance>().HasMany(x=>x.Tasks).WithOne(x=>x.WorkflowInstance).HasForeignKey(x=>x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade); 
        m.Entity<BuilderWorkflowTask>().HasIndex(x=>new{x.TenantId,x.Completed,x.Activated,x.DueAt,x.RetryAt});
        m.Entity<BuilderWorkflowTask>().HasIndex(x => new { x.TenantId, x.Completed, x.AssignedTo, x.CreatedAt });
        m.Entity<BuilderWorkflowInstance>().HasIndex(x => new { x.TenantId, x.CreatedAt });
        m.Entity<BuilderKnowledgeDocument>().HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        m.Entity<BuilderKnowledgeDocument>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Published });
        m.Entity<BuilderSupportTicket>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Status, x.UpdatedAt });
        m.Entity<BuilderSupportComment>().HasIndex(x => new { x.TenantId, x.TicketId, x.CreatedAt });
        m.Entity<BuilderSupportTicket>().HasMany(x => x.Comments).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        m.Entity<BuilderKnowledgeDocument>().Property(x => x.MetadataJson).HasColumnType("jsonb");
        m.Entity<BuilderKnowledgeChunk>().HasIndex(x => new { x.TenantId, x.DocumentId, x.Ordinal }).IsUnique();
        m.Entity<BuilderKnowledgeChunk>().Property(x => x.Embedding).HasColumnType("vector(1536)");
        m.Entity<BuilderKnowledgeChunk>().HasIndex(x => x.Embedding).HasMethod("hnsw").HasOperators("vector_cosine_ops");
    }

    public override int SaveChanges()
    {
        EnsureTenantWrites();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenantWrites();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnsureTenantWrites()
    {
        if (TenantId == Guid.Empty || ChangeTracker.Entries<BuilderEntity>()
                .Any(x => (x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted) && x.Entity.TenantId != TenantId))
            throw new InvalidOperationException("Runtime changes must belong to the selected tenant.");
    }

}
