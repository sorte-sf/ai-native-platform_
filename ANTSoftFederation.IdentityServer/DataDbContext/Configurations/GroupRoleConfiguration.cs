namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class GroupRoleConfiguration : IEntityTypeConfiguration<GroupRole>
{
    public void Configure(EntityTypeBuilder<GroupRole> entity)
    {
        entity.HasKey(ur => new { ur.GroupId, ur.RoleId });
    }
}