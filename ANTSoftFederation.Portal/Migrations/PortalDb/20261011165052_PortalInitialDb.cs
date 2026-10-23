using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace SorteFederation.Portal.Migrations.PortalDb
{
    /// <inheritdoc />
    public partial class PortalInitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ExchangeDiscount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    ParentGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Groups_ParentGroupId",
                        column: x => x.ParentGroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PortalTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    DatabaseName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DatabaseType = table.Column<int>(type: "integer", nullable: false),
                    ConnectionString = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ThemeKey = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ThemeMode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DefaultTheme = table.Column<string>(type: "text", nullable: false),
                    DefaultLanguage = table.Column<string>(type: "text", nullable: false),
                    GlobalFontSize = table.Column<string>(type: "text", nullable: false),
                    ThemeColors = table.Column<string>(type: "text", nullable: false),
                    Descriptions = table.Column<string>(type: "text", nullable: false),
                    AppKey = table.Column<string>(type: "text", nullable: false),
                    RealmKey = table.Column<string>(type: "text", nullable: false),
                    SecretKey = table.Column<string>(type: "text", nullable: false),
                    DbVersion = table.Column<string>(type: "text", nullable: false),
                    AppLogo = table.Column<string>(type: "text", nullable: false),
                    KeySalt = table.Column<string>(type: "text", nullable: false),
                    AllowCrossOrigin = table.Column<string>(type: "text", nullable: false),
                    ConnectionString = table.Column<string>(type: "text", nullable: false),
                    DatabaseType = table.Column<int>(type: "integer", nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "Name", "Descriptions" }),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RegistrationNo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TaxOfficeName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TaxNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ContactPersonName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactPersonTelephoneNo = table.Column<string>(type: "text", nullable: false),
                    ContactPersonEmailAddress = table.Column<string>(type: "text", nullable: false),
                    IsIntegrator = table.Column<bool>(type: "boolean", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThemeKey = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ThemeMode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DailyExchangeRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RateDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CurrencyName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Unit = table.Column<int>(type: "integer", nullable: false),
                    ForexBuying = table.Column<decimal>(type: "numeric", nullable: true),
                    ForexSelling = table.Column<decimal>(type: "numeric", nullable: true),
                    BanknoteBuying = table.Column<decimal>(type: "numeric", nullable: true),
                    BanknoteSelling = table.Column<decimal>(type: "numeric", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyExchangeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyExchangeRates_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationClaims_ApplicationClaims_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ApplicationClaims",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationClaims_ProjectApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProjectApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubTitle = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: true),
                    Roles = table.Column<string>(type: "text", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentMenuId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationMenus_ApplicationMenus_ParentMenuId",
                        column: x => x.ParentMenuId,
                        principalTable: "ApplicationMenus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationMenus_ProjectApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProjectApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Descriptions = table.Column<string>(type: "text", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationModules_ProjectApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProjectApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationVersioning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationVersioning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationVersioning_ProjectApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProjectApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupRoles_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationIntegratorKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationKey = table.Column<string>(type: "text", nullable: false),
                    SecretKey = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationIntegratorKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationIntegratorKeys_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationIntegratorKeys_ProjectApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProjectApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InstallationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: false),
                    ConnectionString = table.Column<string>(type: "text", nullable: false),
                    TenantDatabaseType = table.Column<byte>(type: "smallint", nullable: false),
                    DataBaseType = table.Column<int>(type: "integer", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyApplications_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyApplications_ProjectApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProjectApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    TelePhone = table.Column<string>(type: "text", nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    Password = table.Column<byte[]>(type: "bytea", nullable: true),
                    Salt = table.Column<byte[]>(type: "bytea", nullable: true),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordChangedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TemporaryPassword = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemAccount = table.Column<bool>(type: "boolean", nullable: false),
                    IsLockedOut = table.Column<bool>(type: "boolean", nullable: true),
                    AllowMultipleSessions = table.Column<bool>(type: "boolean", nullable: true),
                    ChangePasswordOnFirstLogin = table.Column<bool>(type: "boolean", nullable: true),
                    TwoFactorType = table.Column<int>(type: "integer", nullable: true),
                    LastLoginDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationModuleIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descriptions = table.Column<string>(type: "text", nullable: false),
                    PrId = table.Column<Guid>(type: "uuid", nullable: true),
                    Solutions = table.Column<string>(type: "text", nullable: false),
                    Keywords = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<byte>(type: "smallint", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketStatus = table.Column<int>(type: "integer", nullable: false),
                    TicketType = table.Column<int>(type: "integer", nullable: false),
                    CustomerSatisfaction = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationModuleIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationModuleIssues_ApplicationModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "ApplicationModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationModuleIssues_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationModuleIssues_ProjectApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProjectApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsSystemPermission = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_ApplicationModules_ApplicationModuleId",
                        column: x => x.ApplicationModuleId,
                        principalTable: "ApplicationModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyApplicationLicences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseKey = table.Column<string>(type: "text", nullable: false),
                    AppLicense = table.Column<string>(type: "text", nullable: false),
                    IssuedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiresAt = table.Column<DateOnly>(type: "date", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseStatus = table.Column<int>(type: "integer", nullable: false),
                    LicenseType = table.Column<int>(type: "integer", nullable: false),
                    CompanyApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyApplicationLicences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyApplicationLicences_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyApplicationLicences_CompanyApplications_CompanyAppli~",
                        column: x => x.CompanyApplicationId,
                        principalTable: "CompanyApplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanyApplicationLicences_ProjectApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProjectApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyAppVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAppVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyAppVersions_ApplicationVersioning_AppVersionId",
                        column: x => x.AppVersionId,
                        principalTable: "ApplicationVersioning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyAppVersions_CompanyApplications_CompanyAppId",
                        column: x => x.CompanyAppId,
                        principalTable: "CompanyApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationDocs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Descriptions = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileLink = table.Column<string>(type: "text", nullable: false),
                    PhysicalFilePath = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationDocs_ApplicationDocs_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ApplicationDocs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationDocs_ApplicationVersioning_AppVersionId",
                        column: x => x.AppVersionId,
                        principalTable: "ApplicationVersioning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationDocs_ProjectApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ProjectApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationDocs_Users_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserApplicationChats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Topic = table.Column<string>(type: "text", nullable: false),
                    ChatDetails = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UserApplicationChatId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApplicationChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserApplicationChats_UserApplicationChats_UserApplicationCh~",
                        column: x => x.UserApplicationChatId,
                        principalTable: "UserApplicationChats",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserApplicationChats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDataGridProperties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Module = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    GroupedFieldsJson = table.Column<string>(type: "text", nullable: false),
                    HiddenColumnsJson = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDataGridProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDataGridProperties_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroups_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    LastAccess = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Started = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationModuleIssueEmbeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueVector = table.Column<float[]>(type: "real[]", maxLength: 1536, nullable: false),
                    ApplicationModuleIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationModuleIssueEmbeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationModuleIssueEmbeddings_ApplicationModuleIssues_Ap~",
                        column: x => x.ApplicationModuleIssueId,
                        principalTable: "ApplicationModuleIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationModuleIssueMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ApplicationModuleIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCustomerMessage = table.Column<bool>(type: "boolean", nullable: false),
                    ParentMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationModuleIssueMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationModuleIssueMessages_ApplicationModuleIssueMessag~",
                        column: x => x.ParentMessageId,
                        principalTable: "ApplicationModuleIssueMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationModuleIssueMessages_ApplicationModuleIssues_Appl~",
                        column: x => x.ApplicationModuleIssueId,
                        principalTable: "ApplicationModuleIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupPermissions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false),
                    Tenant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationEmbeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppVector = table.Column<float[]>(type: "real[]", maxLength: 1536, nullable: false),
                    ApplicationDocId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordState = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationEmbeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationEmbeddings_ApplicationDocs_ApplicationDocId",
                        column: x => x.ApplicationDocId,
                        principalTable: "ApplicationDocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationClaims_ApplicationId",
                table: "ApplicationClaims",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationClaims_ParentId",
                table: "ApplicationClaims",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocs_ApplicationId",
                table: "ApplicationDocs",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocs_AppVersionId",
                table: "ApplicationDocs",
                column: "AppVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocs_ParentId",
                table: "ApplicationDocs",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationDocs_ReviewedById",
                table: "ApplicationDocs",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationEmbeddings_ApplicationDocId",
                table: "ApplicationEmbeddings",
                column: "ApplicationDocId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationIntegratorKeys_ApplicationId",
                table: "ApplicationIntegratorKeys",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationIntegratorKeys_CompanyId",
                table: "ApplicationIntegratorKeys",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMenus_ApplicationId",
                table: "ApplicationMenus",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMenus_ParentMenuId",
                table: "ApplicationMenus",
                column: "ParentMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMenus_Title",
                table: "ApplicationMenus",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModuleIssueEmbeddings_ApplicationModuleIssueId",
                table: "ApplicationModuleIssueEmbeddings",
                column: "ApplicationModuleIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModuleIssueMessages_ApplicationModuleIssueId",
                table: "ApplicationModuleIssueMessages",
                column: "ApplicationModuleIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModuleIssueMessages_ParentMessageId",
                table: "ApplicationModuleIssueMessages",
                column: "ParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModuleIssues_ApplicationId",
                table: "ApplicationModuleIssues",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModuleIssues_CompanyId",
                table: "ApplicationModuleIssues",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModuleIssues_ModuleId",
                table: "ApplicationModuleIssues",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationModules_ApplicationId",
                table: "ApplicationModules",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVersioning_ApplicationId",
                table: "ApplicationVersioning",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CountryId",
                table: "Companies",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyApplicationLicences_ApplicationId",
                table: "CompanyApplicationLicences",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyApplicationLicences_CompanyApplicationId",
                table: "CompanyApplicationLicences",
                column: "CompanyApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyApplicationLicences_CompanyId",
                table: "CompanyApplicationLicences",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyApplications_ApplicationId",
                table: "CompanyApplications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyApplications_CompanyId",
                table: "CompanyApplications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAppVersions_AppVersionId",
                table: "CompanyAppVersions",
                column: "AppVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAppVersions_CompanyAppId",
                table: "CompanyAppVersions",
                column: "CompanyAppId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyExchangeRates_CountryId",
                table: "DailyExchangeRates",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_GroupId",
                table: "GroupPermissions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_PermissionId",
                table: "GroupPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoles_GroupId",
                table: "GroupRoles",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoles_RoleId",
                table: "GroupRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ParentGroupId",
                table: "Groups",
                column: "ParentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ApplicationModuleId",
                table: "Permissions",
                column: "ApplicationModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectApplications_Name",
                table: "ProjectApplications",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectApplications_SearchVector",
                table: "ProjectApplications",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplicationChats_UserApplicationChatId",
                table: "UserApplicationChats",
                column: "UserApplicationChatId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplicationChats_UserId",
                table: "UserApplicationChats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDataGridProperties_UserId",
                table: "UserDataGridProperties",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_GroupId",
                table: "UserGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_UserId",
                table: "UserGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationClaims");

            migrationBuilder.DropTable(
                name: "ApplicationEmbeddings");

            migrationBuilder.DropTable(
                name: "ApplicationIntegratorKeys");

            migrationBuilder.DropTable(
                name: "ApplicationMenus");

            migrationBuilder.DropTable(
                name: "ApplicationModuleIssueEmbeddings");

            migrationBuilder.DropTable(
                name: "ApplicationModuleIssueMessages");

            migrationBuilder.DropTable(
                name: "CompanyApplicationLicences");

            migrationBuilder.DropTable(
                name: "CompanyAppVersions");

            migrationBuilder.DropTable(
                name: "CurrencyTypes");

            migrationBuilder.DropTable(
                name: "DailyExchangeRates");

            migrationBuilder.DropTable(
                name: "GroupPermissions");

            migrationBuilder.DropTable(
                name: "GroupRoles");

            migrationBuilder.DropTable(
                name: "PortalTenants");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserApplicationChats");

            migrationBuilder.DropTable(
                name: "UserDataGridProperties");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "ApplicationDocs");

            migrationBuilder.DropTable(
                name: "ApplicationModuleIssues");

            migrationBuilder.DropTable(
                name: "CompanyApplications");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ApplicationVersioning");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ApplicationModules");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "ProjectApplications");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
