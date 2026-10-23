using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SorteFederation.Builders.Migrations.BuilderDb
{
    /// <inheritdoc />
    public partial class AddPublishedUiFavourites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFavouriteLinks",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    SpecificationJson = table.Column<string>(type: "jsonb", nullable: false),
                    PublishedVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavouriteLinks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouriteLinks_TenantId_ApplicationId_UserId",
                schema: "builders",
                table: "UserFavouriteLinks",
                columns: new[] { "TenantId", "ApplicationId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouriteLinks_TenantId_ApplicationId_UserId_Name",
                schema: "builders",
                table: "UserFavouriteLinks",
                columns: new[] { "TenantId", "ApplicationId", "UserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavouriteLinks",
                schema: "builders");
        }
    }
}
