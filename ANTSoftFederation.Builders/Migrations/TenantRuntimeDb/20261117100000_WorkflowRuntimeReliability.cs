using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SorteFederation.Builders.Data;

#nullable disable

namespace SorteFederation.Builders.Migrations.TenantRuntimeDb;

[DbContext(typeof(TenantRuntimeDbContext))]
[Migration("20261117100000_WorkflowRuntimeReliability")]
public partial class WorkflowRuntimeReliability : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_WorkflowInstances_TenantId_WorkflowId_RecordId",
            schema: "builders_runtime",
            table: "WorkflowInstances");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowInstances_TenantId_WorkflowId_RecordId",
            schema: "builders_runtime",
            table: "WorkflowInstances",
            columns: new[] { "TenantId", "WorkflowId", "RecordId" },
            unique: true,
            filter: "\"Status\" = 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_WorkflowInstances_TenantId_WorkflowId_RecordId",
            schema: "builders_runtime",
            table: "WorkflowInstances");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowInstances_TenantId_WorkflowId_RecordId",
            schema: "builders_runtime",
            table: "WorkflowInstances",
            columns: new[] { "TenantId", "WorkflowId", "RecordId" },
            unique: true);
    }
}
