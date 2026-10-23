
namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal class ApplicationMenuConfiguration : IEntityTypeConfiguration<ApplicationMenu>
{
    public void Configure(EntityTypeBuilder<ApplicationMenu> builder)
    {
        builder.HasIndex(x => x.Title).IsUnique();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);

        builder.HasQueryFilter(e => e.RecordState == RecordState.Active);

        builder.Property(e => e.Roles).HasConversion(v => string.Join(",", v), v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
    }
}