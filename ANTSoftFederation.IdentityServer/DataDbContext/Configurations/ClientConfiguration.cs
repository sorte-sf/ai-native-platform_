
namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;

internal class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> entity)
    {
        entity.HasIndex(c => new { c.ProjectId, c.ClientId }).IsUnique();
    }
}