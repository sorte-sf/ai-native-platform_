namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 

internal class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> entity)
    {
        entity.HasKey(ur => new { ur.UserId, ur.GroupId });
    }
}