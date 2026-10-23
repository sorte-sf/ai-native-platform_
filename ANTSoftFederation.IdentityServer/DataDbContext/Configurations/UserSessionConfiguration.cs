namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> entity)
    { 
        entity.HasKey(ur => new { ur.UserId, ur.ProjectId, ur.SessionId });
    }
}