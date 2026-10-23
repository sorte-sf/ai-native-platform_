namespace SorteFederation.ANTApplications.DataDbContext.Configurations;

internal sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasQueryFilter(x => x.RecordState == RecordState.Active);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.ToTable(table => table.HasCheckConstraint("CK_SubscriptionPlans_Limits", "\"MaxApplications\" > 0 AND \"MaxUsers\" > 0 AND \"DurationMonths\" IN (1, 3, 12)"));
    }
}
