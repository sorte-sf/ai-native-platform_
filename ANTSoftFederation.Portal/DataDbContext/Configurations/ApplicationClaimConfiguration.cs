
namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal class ApplicationClaimConfiguration : IEntityTypeConfiguration<ApplicationClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationClaim> builder)
    {
        builder.HasQueryFilter(e => e.RecordState == RecordState.Active);
    }
}