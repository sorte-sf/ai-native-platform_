namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class UserProjectConfiguration : IEntityTypeConfiguration<UserProject>
{
    public void Configure(EntityTypeBuilder<UserProject> entity)
    { 
        entity.HasKey(ur => new { ur.UserId, ur.ProjectId });
    }
}