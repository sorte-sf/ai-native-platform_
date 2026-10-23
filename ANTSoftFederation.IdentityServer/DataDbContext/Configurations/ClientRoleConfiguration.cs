namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class ClientRoleConfiguration : IEntityTypeConfiguration<ClientRole>
{
    public void Configure(EntityTypeBuilder<ClientRole> entity)
    {
        entity.HasKey(cr => new { cr.Id});
        entity.HasIndex(cr => new { cr.ClientId, cr.RoleId }).IsUnique();
    }
}