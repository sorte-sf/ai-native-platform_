namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal sealed class TenantSubscriptionPaymentConfiguration : IEntityTypeConfiguration<TenantSubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<TenantSubscriptionPayment> builder)
    {
        builder.HasQueryFilter(x => x.RecordState == RecordState.Active);
        builder.HasOne(x => x.Subscription).WithMany().HasForeignKey(x => x.TenantSubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TenantSubscriptionId);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
    }
}
