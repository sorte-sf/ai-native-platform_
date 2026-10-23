using SorteFederation.DesignerStudio.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.DesignerStudio.Data;

public sealed class DesignerStudioDbContext(DbContextOptions<DesignerStudioDbContext> options) : DbContext(options)
{
    public DbSet<TemplatePackage> TemplatePackages => Set<TemplatePackage>();
    public DbSet<TemplatePackageRelease> TemplatePackageReleases => Set<TemplatePackageRelease>();
     
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=Contents/Data/sorteDesignStudio.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TemplatePackage>(entity =>
        {
            entity.ToTable("TemplatePackages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.DraftDefinitionJson).IsRequired();
            entity.Property(x => x.OwnerUserId).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.Key).IsUnique();
            entity.HasIndex(x => new { x.OwnerUserId, x.UpdatedAt });
            entity.HasMany(x => x.Releases).WithOne(x => x.TemplatePackage)
                .HasForeignKey(x => x.TemplatePackageId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplatePackageRelease>(entity =>
        {
            entity.ToTable("TemplatePackageReleases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.ChangeNotes).HasMaxLength(4000);
            entity.Property(x => x.ManifestJson).IsRequired();
            entity.Property(x => x.PackagePath).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CreatedByUserId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ReviewedByUserId).HasMaxLength(200);
            entity.Property(x => x.ReviewNotes).HasMaxLength(4000);
            entity.HasIndex(x => new { x.TemplatePackageId, x.Version }).IsUnique();
            entity.HasIndex(x => new { x.Status, x.PublishedAt });
        });
    }
}
