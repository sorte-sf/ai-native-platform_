using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SorteFederation.Builders.Migrations.BuilderDb
{
    /// <inheritdoc />
    public partial class BuilderInitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "builders");

            migrationBuilder.EnsureSchema(
                name: "ticker");

            migrationBuilder.CreateTable(
                name: "AiTenantPolicies",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Model = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ApiKeyReference = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AuthScheme = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MonthlyTokenLimit = table.Column<long>(type: "bigint", nullable: false),
                    MonthlyRequestLimit = table.Column<int>(type: "integer", nullable: false),
                    MaxOutputTokens = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AiTenantPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiUsage",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Feature = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Model = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    InputTokens = table.Column<long>(type: "bigint", nullable: false),
                    OutputTokens = table.Column<long>(type: "bigint", nullable: false),
                    IsEstimated = table.Column<bool>(type: "boolean", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FailureCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    DurationMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AiUsage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiUsagePeriods",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    UsedTokens = table.Column<long>(type: "bigint", nullable: false),
                    Requests = table.Column<int>(type: "integer", nullable: false),
                    ReservedTokens = table.Column<long>(type: "bigint", nullable: false),
                    ReservedRequests = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiUsagePeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationDeletionRequests",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ApplicationDeletionRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRoles",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ApplicationRoles", x => x.Id);
                    table.UniqueConstraint("AK_ApplicationRoles_TenantId_Id", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    SecurityVersion = table.Column<int>(type: "integer", nullable: false),
                    DisabledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                    table.UniqueConstraint("AK_ApplicationUsers_TenantId_Id", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    ResourceType = table.Column<string>(type: "text", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorId = table.Column<string>(type: "text", nullable: true),
                    DataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConnectorCredentials",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecretReference = table.Column<string>(type: "text", nullable: false),
                    Scheme = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ConnectorCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Connectors",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Connectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CronTickers",
                schema: "ticker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Expression = table.Column<string>(type: "text", nullable: true),
                    Request = table.Column<byte[]>(type: "bytea", nullable: true),
                    Retries = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervals = table.Column<int[]>(type: "integer[]", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemPaused = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Function = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    InitIdentifier = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CronTickers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dashboards",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LayoutJson = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_Dashboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlags",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    RulesJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_FeatureFlags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeDocuments",
                schema: "builders",
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
                name: "Notifications",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    ActionUrl = table.Column<string>(type: "text", nullable: true),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ResourceType = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformEvents",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PlatformEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecordRelationships",
                schema: "builders",
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
                schema: "builders",
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
                name: "Relationships",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Cardinality = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Relationships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DefinitionJson = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DefinitionJson = table.Column<string>(type: "jsonb", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledJobs",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Schedule = table.Column<string>(type: "text", nullable: false),
                    ScheduleType = table.Column<string>(type: "text", nullable: false, defaultValue: "recurring"),
                    RunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TimeZoneId = table.Column<string>(type: "text", nullable: false, defaultValue: "Europe/Istanbul"),
                    NextTickerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    ActionJson = table.Column<string>(type: "jsonb", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ScheduledJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BlueprintVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreviewImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ManifestVersion = table.Column<int>(type: "integer", nullable: false),
                    ManifestJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsBuiltIn = table.Column<bool>(type: "boolean", nullable: false),
                    BaseTemplate = table.Column<string>(type: "text", nullable: true),
                    ThemeJson = table.Column<string>(type: "jsonb", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    HtmlContent = table.Column<string>(type: "text", nullable: false),
                    ScssContent = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                schema: "builders",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "TimeTickers",
                schema: "ticker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Function = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    InitIdentifier = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LockHolder = table.Column<string>(type: "text", nullable: true),
                    Request = table.Column<byte[]>(type: "bytea", nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: true),
                    SkippedReason = table.Column<string>(type: "text", nullable: true),
                    ElapsedTime = table.Column<long>(type: "bigint", nullable: false),
                    Retries = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervals = table.Column<int[]>(type: "integer[]", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RunCondition = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTickers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeTickers_TimeTickers_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "ticker",
                        principalTable: "TimeTickers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                    table.UniqueConstraint("AK_UserGroups_TenantId_Id", x => new { x.TenantId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
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
                name: "Workflows",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DefinitionJson = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuilderWorkspaceAccess",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SecurityVersion = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BuilderWorkspaceAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuilderWorkspaceAccess_ApplicationUsers_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalSchema: "builders",
                        principalTable: "ApplicationUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CronTickerOccurrences",
                schema: "ticker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LockHolder = table.Column<string>(type: "text", nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CronTickerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: true),
                    SkippedReason = table.Column<string>(type: "text", nullable: true),
                    ElapsedTime = table.Column<long>(type: "bigint", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CronTickerOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CronTickerOccurrences_CronTickers_CronTickerId",
                        column: x => x.CronTickerId,
                        principalSchema: "ticker",
                        principalTable: "CronTickers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DashboardWidgets",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DashboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_DashboardWidgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardWidgets_Dashboards_DashboardId",
                        column: x => x.DashboardId,
                        principalSchema: "builders",
                        principalTable: "Dashboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionGrants",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_PermissionGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionGrants_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "builders",
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateReleases",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReleaseVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ManifestVersion = table.Column<int>(type: "integer", nullable: false),
                    ManifestJson = table.Column<string>(type: "jsonb", nullable: true),
                    ThemeJson = table.Column<string>(type: "jsonb", nullable: true),
                    HtmlContent = table.Column<string>(type: "text", nullable: false),
                    ScssContent = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PreviewImageUrl = table.Column<string>(type: "text", nullable: true),
                    BaseTemplate = table.Column<string>(type: "text", nullable: true),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateReleases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateReleases_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "builders",
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantSmtpConfiguration",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    From = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    EnableSsl = table.Column<bool>(type: "boolean", nullable: true),
                    UseStartTls = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_TenantSmtpConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSmtpConfiguration_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "builders",
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupMemberships",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_UserGroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroupMemberships_ApplicationUsers_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalSchema: "builders",
                        principalTable: "ApplicationUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupMemberships_UserGroups_TenantId_GroupId",
                        columns: x => new { x.TenantId, x.GroupId },
                        principalSchema: "builders",
                        principalTable: "UserGroups",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                schema: "builders",
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
                        principalSchema: "builders",
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromState = table.Column<string>(type: "text", nullable: false),
                    ToState = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    ConditionJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "builders",
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNo = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ApplicationType = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateVersion = table.Column<int>(type: "integer", nullable: true),
                    TemplateReleaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppliedTemplateVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ThemeJson = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublishedVersion = table.Column<string>(type: "text", nullable: false),
                    Slugs = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.UniqueConstraint("AK_Applications_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_Applications_TemplateReleases_TemplateReleaseId",
                        column: x => x.TemplateReleaseId,
                        principalSchema: "builders",
                        principalTable: "TemplateReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Applications_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "builders",
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Applications_Tenant_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "builders",
                        principalTable: "Tenant",
                        principalColumn: "TenantId");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationMemberships",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ApplicationMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationMemberships_ApplicationUsers_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalSchema: "builders",
                        principalTable: "ApplicationUsers",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationMemberships_Applications_TenantId_ApplicationId",
                        columns: x => new { x.TenantId, x.ApplicationId },
                        principalSchema: "builders",
                        principalTable: "Applications",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationScreens",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    HtmlContent = table.Column<string>(type: "text", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    FormKey = table.Column<string>(type: "text", nullable: true),
                    FormId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefinitionJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_ApplicationScreens", x => x.Id);
                    table.UniqueConstraint("AK_ApplicationScreens_TenantId_ApplicationId_Id", x => new { x.TenantId, x.ApplicationId, x.Id });
                    table.ForeignKey(
                        name: "FK_ApplicationScreens_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "builders",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationVersions",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    ValidationErrorsJson = table.Column<string>(type: "jsonb", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ApplicationVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationVersions_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "builders",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TemplateType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DefinitionJson = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailTemplates_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "builders",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupRoleAssignments",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_GroupRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupRoleAssignments_ApplicationRoles_TenantId_RoleId",
                        columns: x => new { x.TenantId, x.RoleId },
                        principalSchema: "builders",
                        principalTable: "ApplicationRoles",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupRoleAssignments_Applications_TenantId_ApplicationId",
                        columns: x => new { x.TenantId, x.ApplicationId },
                        principalSchema: "builders",
                        principalTable: "Applications",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupRoleAssignments_UserGroups_TenantId_GroupId",
                        columns: x => new { x.TenantId, x.GroupId },
                        principalSchema: "builders",
                        principalTable: "UserGroups",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Objects",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Objects", x => x.Id);
                    table.UniqueConstraint("AK_Objects_TenantId_ApplicationId_Id", x => new { x.TenantId, x.ApplicationId, x.Id });
                    table.ForeignKey(
                        name: "FK_Objects_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "builders",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantIntegrationConfiguration",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    EnableSsl = table.Column<bool>(type: "boolean", nullable: true),
                    Token = table.Column<string>(type: "text", nullable: true),
                    ConnectionString = table.Column<string>(type: "text", nullable: true),
                    DatabaseName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntegrationType = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_TenantIntegrationConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantIntegrationConfiguration_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "builders",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantIntegrationConfiguration_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "builders",
                        principalTable: "Tenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Searchable = table.Column<bool>(type: "boolean", nullable: false),
                    ReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_Fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fields_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalSchema: "builders",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Forms",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LayoutJson = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Forms", x => x.Id);
                    table.UniqueConstraint("AK_Forms_TenantId_ObjectId_Id", x => new { x.TenantId, x.ObjectId, x.Id });
                    table.ForeignKey(
                        name: "FK_Forms_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalSchema: "builders",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantTableMapping",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationFieldName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    SourceFieldName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    SourceFieldDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TenantIntegrationConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_TenantTableMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantTableMapping_TenantIntegrationConfiguration_TenantInt~",
                        column: x => x.TenantIntegrationConfigurationId,
                        principalSchema: "builders",
                        principalTable: "TenantIntegrationConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationModules",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScreenId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    FormId = table.Column<Guid>(type: "uuid", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ApplicationModules", x => x.Id);
                    table.UniqueConstraint("AK_ApplicationModules_TenantId_ApplicationId_Id", x => new { x.TenantId, x.ApplicationId, x.Id });
                    table.ForeignKey(
                        name: "FK_ApplicationModules_ApplicationScreens_TenantId_ApplicationI~",
                        columns: x => new { x.TenantId, x.ApplicationId, x.ScreenId },
                        principalSchema: "builders",
                        principalTable: "ApplicationScreens",
                        principalColumns: new[] { "TenantId", "ApplicationId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationModules_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "builders",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationModules_Forms_TenantId_ObjectId_FormId",
                        columns: x => new { x.TenantId, x.ObjectId, x.FormId },
                        principalSchema: "builders",
                        principalTable: "Forms",
                        principalColumns: new[] { "TenantId", "ObjectId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationModules_Objects_TenantId_ApplicationId_ObjectId",
                        columns: x => new { x.TenantId, x.ApplicationId, x.ObjectId },
                        principalSchema: "builders",
                        principalTable: "Objects",
                        principalColumns: new[] { "TenantId", "ApplicationId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationMenuItems",
                schema: "builders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Route = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Visible = table.Column<bool>(type: "boolean", nullable: false),
                    PermissionKey = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ApplicationMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationMenuItems_ApplicationMenuItems_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "builders",
                        principalTable: "ApplicationMenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationMenuItems_ApplicationModules_TenantId_Applicatio~",
                        columns: x => new { x.TenantId, x.ApplicationId, x.ModuleId },
                        principalSchema: "builders",
                        principalTable: "ApplicationModules",
                        principalColumns: new[] { "TenantId", "ApplicationId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationMenuItems_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "builders",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiTenantPolicies_TenantId",
                schema: "builders",
                table: "AiTenantPolicies",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiUsage_RequestId",
                schema: "builders",
                table: "AiUsage",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiUsage_TenantId_CreatedAt",
                schema: "builders",
                table: "AiUsage",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AiUsage_TenantId_Feature_CreatedAt",
                schema: "builders",
                table: "AiUsage",
                columns: new[] { "TenantId", "Feature", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AiUsagePeriods_TenantId_PeriodStart",
                schema: "builders",
                table: "AiUsagePeriods",
                columns: new[] { "TenantId", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDeletionRequests_ApplicationId_Status",
                schema: "builders",
                table: "ApplicationDeletionRequests",
                columns: new[] { "ApplicationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMemberships_TenantId_ApplicationId_UserId",
                schema: "builders",
                table: "ApplicationMemberships",
                columns: new[] { "TenantId", "ApplicationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMemberships_TenantId_UserId",
                schema: "builders",
                table: "ApplicationMemberships",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMenuItems_ApplicationId",
                schema: "builders",
                table: "ApplicationMenuItems",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMenuItems_ParentId",
                schema: "builders",
                table: "ApplicationMenuItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMenuItems_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "ApplicationMenuItems",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMenuItems_TenantId_ApplicationId_ModuleId",
                schema: "builders",
                table: "ApplicationMenuItems",
                columns: new[] { "TenantId", "ApplicationId", "ModuleId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModules_ApplicationId",
                schema: "builders",
                table: "ApplicationModules",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModules_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "ApplicationModules",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModules_TenantId_ApplicationId_ObjectId",
                schema: "builders",
                table: "ApplicationModules",
                columns: new[] { "TenantId", "ApplicationId", "ObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModules_TenantId_ApplicationId_ScreenId",
                schema: "builders",
                table: "ApplicationModules",
                columns: new[] { "TenantId", "ApplicationId", "ScreenId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModules_TenantId_ObjectId_FormId",
                schema: "builders",
                table: "ApplicationModules",
                columns: new[] { "TenantId", "ObjectId", "FormId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_TenantId_Key",
                schema: "builders",
                table: "ApplicationRoles",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CompanyId",
                schema: "builders",
                table: "Applications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_TemplateId",
                schema: "builders",
                table: "Applications",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_TemplateReleaseId",
                schema: "builders",
                table: "Applications",
                column: "TemplateReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_TenantId_CreatedByUserId",
                schema: "builders",
                table: "Applications",
                columns: new[] { "TenantId", "CreatedByUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_TenantId_Key",
                schema: "builders",
                table: "Applications",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationScreens_ApplicationId",
                schema: "builders",
                table: "ApplicationScreens",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationScreens_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "ApplicationScreens",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_TenantId_NormalizedEmail",
                schema: "builders",
                table: "ApplicationUsers",
                columns: new[] { "TenantId", "NormalizedEmail" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVersions_ApplicationId",
                schema: "builders",
                table: "ApplicationVersions",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVersions_TenantId_ApplicationId_VersionNumber",
                schema: "builders",
                table: "ApplicationVersions",
                columns: new[] { "TenantId", "ApplicationId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_TenantId_CreatedAt",
                schema: "builders",
                table: "AuditEvents",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BuilderWorkspaceAccess_TenantId_UserId",
                schema: "builders",
                table: "BuilderWorkspaceAccess",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectorCredentials_TenantId_ConnectorId_Scheme",
                schema: "builders",
                table: "ConnectorCredentials",
                columns: new[] { "TenantId", "ConnectorId", "Scheme" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Connectors_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "Connectors",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_CronTickerId",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "CronTickerId");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                column: "ExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_Status_ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                columns: new[] { "Status", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "UQ_CronTickerId_ExecutionTime",
                schema: "ticker",
                table: "CronTickerOccurrences",
                columns: new[] { "CronTickerId", "ExecutionTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CronTickers_Expression",
                schema: "ticker",
                table: "CronTickers",
                column: "Expression");

            migrationBuilder.CreateIndex(
                name: "IX_Function_Expression",
                schema: "ticker",
                table: "CronTickers",
                columns: new[] { "Function", "Expression" });

            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "Dashboards",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgets_DashboardId",
                schema: "builders",
                table: "DashboardWidgets",
                column: "DashboardId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgets_TenantId_DashboardId_Key",
                schema: "builders",
                table: "DashboardWidgets",
                columns: new[] { "TenantId", "DashboardId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_ApplicationId",
                schema: "builders",
                table: "EmailTemplates",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "EmailTemplates",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "FeatureFlags",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fields_ObjectId",
                schema: "builders",
                table: "Fields",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_TenantId_ObjectId_Key",
                schema: "builders",
                table: "Fields",
                columns: new[] { "TenantId", "ObjectId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forms_ObjectId",
                schema: "builders",
                table: "Forms",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_TenantId_ObjectId_Key",
                schema: "builders",
                table: "Forms",
                columns: new[] { "TenantId", "ObjectId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleAssignments_TenantId_ApplicationId_GroupId_RoleId",
                schema: "builders",
                table: "GroupRoleAssignments",
                columns: new[] { "TenantId", "ApplicationId", "GroupId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleAssignments_TenantId_GroupId",
                schema: "builders",
                table: "GroupRoleAssignments",
                columns: new[] { "TenantId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleAssignments_TenantId_RoleId",
                schema: "builders",
                table: "GroupRoleAssignments",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeDocuments_TenantId_Key",
                schema: "builders",
                table: "KnowledgeDocuments",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TenantId_RecipientId_ReadAt",
                schema: "builders",
                table: "Notifications",
                columns: new[] { "TenantId", "RecipientId", "ReadAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Objects_ApplicationId",
                schema: "builders",
                table: "Objects",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "Objects",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionGrants_PermissionId",
                schema: "builders",
                table: "PermissionGrants",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionGrants_TenantId_PermissionId_SubjectType_SubjectId",
                schema: "builders",
                table: "PermissionGrants",
                columns: new[] { "TenantId", "PermissionId", "SubjectType", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "Permissions",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformEvents_TenantId_Status_CreatedAt",
                schema: "builders",
                table: "PlatformEvents",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RecordRelationships_TenantId_RelationshipId_SourceRecordId_~",
                schema: "builders",
                table: "RecordRelationships",
                columns: new[] { "TenantId", "RelationshipId", "SourceRecordId", "TargetRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_TenantId_SourceObjectId_Key",
                schema: "builders",
                table: "Relationships",
                columns: new[] { "TenantId", "SourceObjectId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "Reports",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rules_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "Rules",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "ScheduledJobs",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateReleases_TemplateId_ReleaseVersion",
                schema: "builders",
                table: "TemplateReleases",
                columns: new[] { "TemplateId", "ReleaseVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Category",
                schema: "builders",
                table: "Templates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Key",
                schema: "builders",
                table: "Templates",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Name",
                schema: "builders",
                table: "Templates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TenantIntegrationConfiguration_ApplicationId",
                schema: "builders",
                table: "TenantIntegrationConfiguration",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantIntegrationConfiguration_TenantId_ApplicationId_Name",
                schema: "builders",
                table: "TenantIntegrationConfiguration",
                columns: new[] { "TenantId", "ApplicationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSmtpConfiguration_TenantId",
                schema: "builders",
                table: "TenantSmtpConfiguration",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantTableMapping_TenantIntegrationConfigurationId_SourceF~",
                schema: "builders",
                table: "TenantTableMapping",
                columns: new[] { "TenantIntegrationConfigurationId", "SourceFieldName", "DestinationFieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                column: "ExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "ticker",
                table: "TimeTickers",
                columns: new[] { "Status", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTickers_ParentId",
                schema: "ticker",
                table: "TimeTickers",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberships_TenantId_GroupId_UserId",
                schema: "builders",
                table: "UserGroupMemberships",
                columns: new[] { "TenantId", "GroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberships_TenantId_UserId",
                schema: "builders",
                table: "UserGroupMemberships",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_TenantId_Key",
                schema: "builders",
                table: "UserGroups",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_TenantId_WorkflowId_RecordId",
                schema: "builders",
                table: "WorkflowInstances",
                columns: new[] { "TenantId", "WorkflowId", "RecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_Status_TenantId",
                schema: "builders",
                table: "Workflows",
                columns: new[] { "Status", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_TenantId_ApplicationId_Key",
                schema: "builders",
                table: "Workflows",
                columns: new[] { "TenantId", "ApplicationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowInstanceId",
                schema: "builders",
                table: "WorkflowTasks",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_WorkflowId",
                schema: "builders",
                table: "WorkflowTransitions",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiTenantPolicies",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "AiUsage",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "AiUsagePeriods",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ApplicationDeletionRequests",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ApplicationMemberships",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ApplicationMenuItems",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ApplicationVersions",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "AuditEvents",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "BuilderWorkspaceAccess",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ConnectorCredentials",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Connectors",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "CronTickerOccurrences",
                schema: "ticker");

            migrationBuilder.DropTable(
                name: "DashboardWidgets",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "EmailTemplates",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "FeatureFlags",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Fields",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "GroupRoleAssignments",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "KnowledgeDocuments",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "PermissionGrants",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "PlatformEvents",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "RecordRelationships",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Records",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Relationships",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Reports",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Rules",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ScheduledJobs",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "TenantSmtpConfiguration",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "TenantTableMapping",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "TimeTickers",
                schema: "ticker");

            migrationBuilder.DropTable(
                name: "UserGroupMemberships",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "WorkflowTasks",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ApplicationModules",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "CronTickers",
                schema: "ticker");

            migrationBuilder.DropTable(
                name: "Dashboards",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ApplicationRoles",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "TenantIntegrationConfiguration",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ApplicationUsers",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "UserGroups",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "WorkflowInstances",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Workflows",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "ApplicationScreens",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Forms",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Objects",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Applications",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "TemplateReleases",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Tenant",
                schema: "builders");

            migrationBuilder.DropTable(
                name: "Templates",
                schema: "builders");
        }
    }
}
