namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(u => u.Id);
        entity.HasIndex(u => new { u.Email, u.Username }).IsUnique();
    }
}