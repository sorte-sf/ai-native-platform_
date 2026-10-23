namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> entity)
    { 
        entity.HasKey(ur => new { ur.UserId, ur.RoleId });
    }
}