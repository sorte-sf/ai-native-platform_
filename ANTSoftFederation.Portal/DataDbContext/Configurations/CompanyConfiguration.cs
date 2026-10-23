
namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    { 
        builder.HasQueryFilter(e => e.RecordState == RecordState.Active);
    }
}