using SorteFederation.ANTApplications.DataDbContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SorteFederation.Portal.Migrations.PortalDb;

[DbContext(typeof(ANTSoftPortalDbContext))]
[Migration("20261019224500_AddSubscriptionTermsSnapshot")]
public sealed class AddSubscriptionTermsSnapshot : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "PlanName", table: "TenantSubscriptions", type: "character varying(120)", maxLength: 120, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<int>(name: "MaxApplications", table: "TenantSubscriptions", type: "integer", nullable: false, defaultValue: 1);
        migrationBuilder.AddColumn<int>(name: "MaxUsers", table: "TenantSubscriptions", type: "integer", nullable: false, defaultValue: 1);
        migrationBuilder.AddColumn<int>(name: "DurationMonths", table: "TenantSubscriptions", type: "integer", nullable: false, defaultValue: 1);
        migrationBuilder.AddColumn<decimal>(name: "PlanPrice", table: "TenantSubscriptions", type: "numeric(18,2)", precision: 18, scale: 2, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PlanCurrency", table: "TenantSubscriptions", type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD");

        migrationBuilder.Sql("""
            UPDATE "TenantSubscriptions" AS subscription
            SET "PlanName" = plan."Name",
                "MaxApplications" = plan."MaxApplications",
                "MaxUsers" = plan."MaxUsers",
                "DurationMonths" = plan."DurationMonths",
                "PlanPrice" = plan."Price",
                "PlanCurrency" = plan."Currency"
            FROM "SubscriptionPlans" AS plan
            WHERE subscription."SubscriptionPlanId" = plan."Id";
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PlanName", table: "TenantSubscriptions");
        migrationBuilder.DropColumn(name: "MaxApplications", table: "TenantSubscriptions");
        migrationBuilder.DropColumn(name: "MaxUsers", table: "TenantSubscriptions");
        migrationBuilder.DropColumn(name: "DurationMonths", table: "TenantSubscriptions");
        migrationBuilder.DropColumn(name: "PlanPrice", table: "TenantSubscriptions");
        migrationBuilder.DropColumn(name: "PlanCurrency", table: "TenantSubscriptions");
    }
}
