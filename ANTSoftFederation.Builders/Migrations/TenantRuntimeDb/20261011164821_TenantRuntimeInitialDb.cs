using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace SorteFederation.Builders.Migrations.TenantRuntimeDb
{
    /// <inheritdoc />
    public partial class TenantRuntimeInitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "builders_runtime");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "KnowledgeChunks",
                schema: "builders_runtime",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordinal = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SourceUri = table.Column<string>(type: "text", nullable: true),
                    EmbeddingModel = table.Column<string>(type: "text", nullable: true),
                    EmbeddingDimensions = table.Column<int>(type: "integer", nullable: true),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    IndexStatus = table.Column<string>(type: "text", nullable: false),
                    IndexedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_KnowledgeChunks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeDocuments",
                schema: "builders_runtime",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_KnowledgeDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecordRelationships",
                schema: "builders_runtime",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetRecordId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_RecordRelationships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Records",
                schema: "builders_runtime",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataJson = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_Records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                schema: "builders_runtime",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    OpenedBy = table.Column<string>(type: "text", nullable: false),
                    AssignedTo = table.Column<string>(type: "text", nullable: true),
                    Resolution = table.Column<string>(type: "text", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    KnowledgeDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                schema: "builders_runtime",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentState = table.Column<string>(type: "text", nullable: false),
                    NextWakeAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    RecoveryAttempts = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportComments",
                schema: "builders_runtime",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_SupportComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportComments_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "builders_runtime",
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                schema: "builders_runtime",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AssignedTo = table.Column<string>(type: "text", nullable: true),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EscalateAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EscalateTo = table.Column<string>(type: "text", nullable: true),
                    BranchKey = table.Column<string>(type: "text", nullable: true),
                    DependsOn = table.Column<string>(type: "text", nullable: true),
                    JoinKey = table.Column<string>(type: "text", nullable: true),
                    Activated = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    RetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "builders_runtime",
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeChunks_Embedding",
                schema: "builders_runtime",
                table: "KnowledgeChunks",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeChunks_TenantId_DocumentId_Ordinal",
                schema: "builders_runtime",
                table: "KnowledgeChunks",
                columns: new[] { "TenantId", "DocumentId", "Ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_TenantId_ApplicationId_Published",
                schema: "builders_runtime",
                table: "KnowledgeDocuments",
                columns: new[] { "TenantId", "ApplicationId", "Published" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_TenantId_Key",
                schema: "builders_runtime",
                table: "KnowledgeDocuments",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordRelationships_TenantId_RelationshipId_SourceRecordId_~",
                schema: "builders_runtime",
                table: "RecordRelationships",
                columns: new[] { "TenantId", "RelationshipId", "SourceRecordId", "TargetRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Records_TenantId_ObjectId_UpdatedAt",
                schema: "builders_runtime",
                table: "Records",
                columns: new[] { "TenantId", "ObjectId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportComments_TenantId_TicketId_CreatedAt",
                schema: "builders_runtime",
                table: "SupportComments",
                columns: new[] { "TenantId", "TicketId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportComments_TicketId",
                schema: "builders_runtime",
                table: "SupportComments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TenantId_ApplicationId_Status_UpdatedAt",
                schema: "builders_runtime",
                table: "SupportTickets",
                columns: new[] { "TenantId", "ApplicationId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId_CreatedAt",
                schema: "builders_runtime",
                table: "WorkflowInstances",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId_WorkflowId_RecordId",
                schema: "builders_runtime",
                table: "WorkflowInstances",
                columns: new[] { "TenantId", "WorkflowId", "RecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_TenantId_Completed_Activated_DueAt_RetryAt",
                schema: "builders_runtime",
                table: "WorkflowTasks",
                columns: new[] { "TenantId", "Completed", "Activated", "DueAt", "RetryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_TenantId_Completed_AssignedTo_CreatedAt",
                schema: "builders_runtime",
                table: "WorkflowTasks",
                columns: new[] { "TenantId", "Completed", "AssignedTo", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowInstanceId",
                schema: "builders_runtime",
                table: "WorkflowTasks",
                column: "WorkflowInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnowledgeChunks",
                schema: "builders_runtime");

            migrationBuilder.DropTable(
                name: "KnowledgeDocuments",
                schema: "builders_runtime");

            migrationBuilder.DropTable(
                name: "RecordRelationships",
                schema: "builders_runtime");

            migrationBuilder.DropTable(
                name: "Records",
                schema: "builders_runtime");

            migrationBuilder.DropTable(
                name: "SupportComments",
                schema: "builders_runtime");

            migrationBuilder.DropTable(
                name: "WorkflowTasks",
                schema: "builders_runtime");

            migrationBuilder.DropTable(
                name: "SupportTickets",
                schema: "builders_runtime");

            migrationBuilder.DropTable(
                name: "WorkflowInstances",
                schema: "builders_runtime");
        }
    }
}
