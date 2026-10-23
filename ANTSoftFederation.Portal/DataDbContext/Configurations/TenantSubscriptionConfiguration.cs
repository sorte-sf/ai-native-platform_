namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.HasQueryFilter(x => x.RecordState == RecordState.Active);
        builder.HasOne(x => x.PortalTenant).WithMany().HasForeignKey(x => x.PortalTenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.SubscriptionPlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.PortalTenantId, x.Status, x.EndsOn });
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.PlanName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PlanPrice).HasPrecision(18, 2);
        builder.Property(x => x.PlanCurrency).HasMaxLength(3).IsRequired();
    }
}
