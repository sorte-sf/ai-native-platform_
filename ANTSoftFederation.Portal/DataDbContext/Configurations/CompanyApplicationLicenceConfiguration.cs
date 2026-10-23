
namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal class CompanyApplicationLicenceConfiguration : IEntityTypeConfiguration<CompanyApplicationLicence>
{
    public void Configure(EntityTypeBuilder<CompanyApplicationLicence> builder)
    { 
        builder.HasQueryFilter(e => e.RecordState == RecordState.Active);
    }
}