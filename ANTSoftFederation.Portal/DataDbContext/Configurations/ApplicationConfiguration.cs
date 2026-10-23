
namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal class ApplicationConfiguration : IEntityTypeConfiguration<ProjectApplication>
{
    public void Configure(EntityTypeBuilder<ProjectApplication> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);

        builder.HasQueryFilter(e => e.RecordState == RecordState.Active);

        builder.HasGeneratedTsVectorColumn(
            p => p.SearchVector,
            "english",
            p => new { p.Name, p.Descriptions  })
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN"); // Index

    }
}