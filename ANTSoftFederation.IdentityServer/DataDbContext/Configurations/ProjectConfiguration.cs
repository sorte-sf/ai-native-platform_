namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> entity)
    {
        entity.HasIndex(r => r.Name).IsUnique();
    }
}