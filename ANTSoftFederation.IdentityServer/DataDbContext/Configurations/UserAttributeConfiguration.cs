namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class UserAttributeConfiguration : IEntityTypeConfiguration<UserAttribute>
{
    public void Configure(EntityTypeBuilder<UserAttribute> entity)
    { 
        entity.HasKey(ur => new { ur.UserId, ur.Name }); 
    }
}