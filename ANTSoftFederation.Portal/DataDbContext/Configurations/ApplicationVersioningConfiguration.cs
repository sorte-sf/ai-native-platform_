
namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal class ApplicationVersioningConfiguration : IEntityTypeConfiguration<ApplicationVersioning>
{
    public void Configure(EntityTypeBuilder<ApplicationVersioning> builder)
    {
        builder.HasQueryFilter(e => e.RecordState == RecordState.Active);
    }
}