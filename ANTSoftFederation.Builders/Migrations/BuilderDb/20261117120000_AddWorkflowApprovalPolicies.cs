using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SorteFederation.Builders.Data;

#nullable disable

namespace SorteFederation.Builders.Migrations.BuilderDb;

[DbContext(typeof(BuilderDbContext))]
[Migration("20261117120000_AddWorkflowApprovalPolicies")]
public sealed class AddWorkflowApprovalPolicies : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WorkflowApprovalPolicies",
            schema: "builders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                Key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                StepsJson = table.Column<string>(type: "jsonb", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Version = table.Column<int>(type: "integer", nullable: false),
                CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                RecordState = table.Column<byte>(type: "smallint", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_WorkflowApprovalPolicies", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowApprovalPolicies_TenantId_ApplicationId_Key",
            schema: "builders", table: "WorkflowApprovalPolicies",
            columns: new[] { "TenantId", "ApplicationId", "Key" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "WorkflowApprovalPolicies", schema: "builders");
}
