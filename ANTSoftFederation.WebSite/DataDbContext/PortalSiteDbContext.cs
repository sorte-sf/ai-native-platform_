using EntityFrameworkCore.AutoHistory;
using EntityFrameworkCore.AutoHistory.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.WebSite.DataDbContext;

public partial class PortalSiteDbContext : DbContext
{
    public PortalSiteDbContext() { }

    public PortalSiteDbContext(
        DbContextOptions<PortalSiteDbContext> options
        )
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    //public virtual DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.EnableAutoHistory<AutoHistory>((options) =>
        {
            options.RowIdMaxLength = 128;
            options.ChangedMaxLength = 8096 * 2;
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortalSiteDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        this.EnsureAutoHistory();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.EnsureAutoHistory();

        return await base.SaveChangesAsync(cancellationToken);
    }


}
