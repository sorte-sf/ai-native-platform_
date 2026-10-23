namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 

internal class ProjectGroupConfiguration : IEntityTypeConfiguration<ProjectGroup>
{
    public void Configure(EntityTypeBuilder<ProjectGroup> entity)
    {
        entity.HasKey(ur => new { ur.ProjectId, ur.GroupId });
    }
}