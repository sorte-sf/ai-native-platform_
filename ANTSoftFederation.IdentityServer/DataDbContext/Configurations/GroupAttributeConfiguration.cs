namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class GroupAttributeConfiguration : IEntityTypeConfiguration<GroupAttribute>
{
    public void Configure(EntityTypeBuilder<GroupAttribute> entity)
    {
        entity.HasIndex(ga => new { ga.GroupId, ga.Name });
    }
}