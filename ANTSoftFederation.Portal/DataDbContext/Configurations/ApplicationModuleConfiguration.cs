
namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal class ApplicationModuleConfiguration : IEntityTypeConfiguration<ApplicationModule>
{
    public void Configure(EntityTypeBuilder<ApplicationModule> builder)
    {
        builder.HasQueryFilter(e => e.RecordState == RecordState.Active);
    }
}