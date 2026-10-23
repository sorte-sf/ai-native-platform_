using SorteFederation.ANTApplications.DataDbContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SorteFederation.Portal.Migrations.PortalDb;

[DbContext(typeof(ANTSoftPortalDbContext))]
[Migration("20261019223000_AddTenantSubscriptions")]
public sealed class AddTenantSubscriptions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SubscriptionPlans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                MaxApplications = table.Column<int>(type: "integer", nullable: false),
                MaxUsers = table.Column<int>(type: "integer", nullable: false),
                DurationMonths = table.Column<int>(type: "integer", nullable: false),
                Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                RecordState = table.Column<byte>(type: "smallint", nullable: false),
                Tenant = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                table.CheckConstraint("CK_SubscriptionPlans_Limits", "\"MaxApplications\" > 0 AND \"MaxUsers\" > 0 AND \"DurationMonths\" IN (1, 3, 12)");
            });

        migrationBuilder.CreateTable(
            name: "TenantSubscriptions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PortalTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                RecordState = table.Column<byte>(type: "smallint", nullable: false),
                Tenant = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TenantSubscriptions", x => x.Id);
                table.ForeignKey("FK_TenantSubscriptions_PortalTenants_PortalTenantId", x => x.PortalTenantId, "PortalTenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_TenantSubscriptions_SubscriptionPlans_SubscriptionPlanId", x => x.SubscriptionPlanId, "SubscriptionPlans", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TenantSubscriptionPayments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                PaymentMethod = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                ProviderReference = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                RecordState = table.Column<byte>(type: "smallint", nullable: false),
                Tenant = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TenantSubscriptionPayments", x => x.Id);
                table.ForeignKey("FK_TenantSubscriptionPayments_TenantSubscriptions_TenantSubscriptionId", x => x.TenantSubscriptionId, "TenantSubscriptions", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_SubscriptionPlans_Name", "SubscriptionPlans", "Name", unique: true);
        migrationBuilder.CreateIndex("IX_TenantSubscriptions_PortalTenantId_Status_EndsOn", "TenantSubscriptions", new[] { "PortalTenantId", "Status", "EndsOn" });
        migrationBuilder.CreateIndex("IX_TenantSubscriptions_SubscriptionPlanId", "TenantSubscriptions", "SubscriptionPlanId");
        migrationBuilder.CreateIndex("IX_TenantSubscriptionPayments_TenantSubscriptionId", "TenantSubscriptionPayments", "TenantSubscriptionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TenantSubscriptionPayments");
        migrationBuilder.DropTable(name: "TenantSubscriptions");
        migrationBuilder.DropTable(name: "SubscriptionPlans");
    }
}
