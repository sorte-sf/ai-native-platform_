using SorteFederation.DesignerStudio.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SorteFederation.DesignerStudio.Migrations;

[DbContext(typeof(DesignerStudioDbContext))]
[Migration("20261017000000_InitialSqlite")]
public sealed class InitialSqlite : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TemplatePackages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                Category = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                DraftDefinitionJson = table.Column<string>(type: "TEXT", nullable: false),
                OwnerUserId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_TemplatePackages", x => x.Id));

        migrationBuilder.CreateTable(
            name: "TemplatePackageReleases",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                TemplatePackageId = table.Column<Guid>(type: "TEXT", nullable: false),
                Version = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                ManifestJson = table.Column<string>(type: "TEXT", nullable: false),
                PackagePath = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                ChangeNotes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                CreatedByUserId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                ReviewedByUserId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                ReviewNotes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                SubmittedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                ReviewedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                PublishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TemplatePackageReleases", x => x.Id);
                table.ForeignKey(
                    name: "FK_TemplatePackageReleases_TemplatePackages_TemplatePackageId",
                    column: x => x.TemplatePackageId,
                    principalTable: "TemplatePackages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TemplatePackageReleases_Status_PublishedAt",
            table: "TemplatePackageReleases",
            columns: new[] { "Status", "PublishedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_TemplatePackageReleases_TemplatePackageId_Version",
            table: "TemplatePackageReleases",
            columns: new[] { "TemplatePackageId", "Version" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TemplatePackages_Key",
            table: "TemplatePackages",
            column: "Key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TemplatePackages_OwnerUserId_UpdatedAt",
            table: "TemplatePackages",
            columns: new[] { "OwnerUserId", "UpdatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TemplatePackageReleases");
        migrationBuilder.DropTable(name: "TemplatePackages");
    }
}
