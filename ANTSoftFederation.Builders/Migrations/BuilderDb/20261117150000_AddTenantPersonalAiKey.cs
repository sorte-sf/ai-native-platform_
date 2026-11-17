using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SorteFederation.Builders.Data;

#nullable disable

namespace SorteFederation.Builders.Migrations.BuilderDb;

[DbContext(typeof(BuilderDbContext))]
[Migration("20261117150000_AddTenantPersonalAiKey")]
public sealed class AddTenantPersonalAiKey : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "PersonalApiKey", schema: "builders", table: "AiTenantPolicies",
            type: "character varying(4096)", maxLength: 4096, nullable: true);

        migrationBuilder.CreateTable(name: "AiUserModels", schema: "builders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Model = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                ApiKey = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                AuthScheme = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Version = table.Column<int>(type: "integer", nullable: false),
                CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                RecordState = table.Column<byte>(type: "smallint", nullable: false)
            }, constraints: table => table.PrimaryKey("PK_AiUserModels", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_AiUserModels_TenantId_UserId", schema: "builders", table: "AiUserModels",
            columns: new[] { "TenantId", "UserId" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AiUserModels", schema: "builders");
        migrationBuilder.DropColumn(name: "PersonalApiKey", schema: "builders", table: "AiTenantPolicies");
    }
}
