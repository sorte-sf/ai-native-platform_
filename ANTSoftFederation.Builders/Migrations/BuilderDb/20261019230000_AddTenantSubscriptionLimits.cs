using SorteFederation.Builders.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SorteFederation.Builders.Migrations.BuilderDb;

[DbContext(typeof(BuilderDbContext))]
[Migration("20261019230000_AddTenantSubscriptionLimits")]
public sealed class AddTenantSubscriptionLimits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(name: "HasActiveSubscription", schema: "builders", table: "Tenant", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>(name: "SubscriptionMaxApplications", schema: "builders", table: "Tenant", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>(name: "SubscriptionMaxUsers", schema: "builders", table: "Tenant", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<DateOnly>(name: "SubscriptionStartsOn", schema: "builders", table: "Tenant", type: "date", nullable: true);
        migrationBuilder.AddColumn<DateOnly>(name: "SubscriptionEndsOn", schema: "builders", table: "Tenant", type: "date", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "HasActiveSubscription", schema: "builders", table: "Tenant");
        migrationBuilder.DropColumn(name: "SubscriptionMaxApplications", schema: "builders", table: "Tenant");
        migrationBuilder.DropColumn(name: "SubscriptionMaxUsers", schema: "builders", table: "Tenant");
        migrationBuilder.DropColumn(name: "SubscriptionStartsOn", schema: "builders", table: "Tenant");
        migrationBuilder.DropColumn(name: "SubscriptionEndsOn", schema: "builders", table: "Tenant");
    }
}
