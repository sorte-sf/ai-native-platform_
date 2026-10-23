
namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal class ApplicationDocConfiguration : IEntityTypeConfiguration<ApplicationDoc>
{
    public void Configure(EntityTypeBuilder<ApplicationDoc> builder)
    { 
        builder.HasQueryFilter(e => e.RecordState == RecordState.Active);
    }
}