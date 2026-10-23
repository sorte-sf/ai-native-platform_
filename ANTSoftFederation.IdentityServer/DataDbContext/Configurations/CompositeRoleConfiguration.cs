namespace ANTSoftFederation.IdentityServer.DataDbContext.Configurations;
 
internal class CompositeRoleConfiguration : IEntityTypeConfiguration<CompositeRole>
{
    public void Configure(EntityTypeBuilder<CompositeRole> entity)
    {
        entity.HasIndex(cr => new { cr.Id }).IsUnique();
        entity.HasKey(cr => new { cr.ParentRoleId, cr.ChildRoleId });

        entity.HasOne(cr => cr.ParentRole)
            .WithMany()
            .HasForeignKey(cr => cr.ParentRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(cr => cr.ChildRole)
            .WithMany()
            .HasForeignKey(cr => cr.ChildRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}