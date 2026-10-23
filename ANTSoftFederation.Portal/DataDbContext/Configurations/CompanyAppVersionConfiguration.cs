
namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal class CompanyAppVersionConfiguration : IEntityTypeConfiguration<CompanyAppVersion>
{
    public void Configure(EntityTypeBuilder<CompanyAppVersion> builder)
    { 
        builder.HasQueryFilter(e => e.RecordState == RecordState.Active);
    }
}