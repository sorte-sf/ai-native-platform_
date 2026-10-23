using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SorteFederation.BlobApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RowId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TableName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Changed = table.Column<string>(type: "character varying(16192)", maxLength: 16192, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentCategoreis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentCategoreis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentCategoreis_DocumentCategoreis_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "DocumentCategoreis",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentWorkflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    AssociatedWorkflowId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTemplates_DocumentWorkflows_AssociatedWorkflowId",
                        column: x => x.AssociatedWorkflowId,
                        principalTable: "DocumentWorkflows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentWorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    ResponsibleRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequiredAction = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    DeadlineDays = table.Column<int>(type: "integer", nullable: true),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false),
                    ResponsibleRole = table.Column<string>(type: "text", nullable: false),
                    ResponsibleUser = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentWorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowSteps_DocumentWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "DocumentWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentTemplateMetadataFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ValidationPattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HelpText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplateMetadataFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTemplateMetadataFields_DocumentTemplates_DocumentTe~",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentAccessControls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PermissionLevel = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAccessControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentAccessControls_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentDocumentCategory",
                columns: table => new
                {
                    DocumentCategoriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentDocumentCategory", x => new { x.DocumentCategoriesId, x.DocumentsId });
                    table.ForeignKey(
                        name: "FK_DocumentDocumentCategory_DocumentCategoreis_DocumentCategor~",
                        column: x => x.DocumentCategoriesId,
                        principalTable: "DocumentCategoreis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentDocumentCategory_Documents_DocumentsId",
                        column: x => x.DocumentsId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentDocumentTag",
                columns: table => new
                {
                    DocumentTagsId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentDocumentTag", x => new { x.DocumentTagsId, x.DocumentsId });
                    table.ForeignKey(
                        name: "FK_DocumentDocumentTag_DocumentTags_DocumentTagsId",
                        column: x => x.DocumentTagsId,
                        principalTable: "DocumentTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentDocumentTag_Documents_DocumentsId",
                        column: x => x.DocumentsId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VersionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentVersions_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentWorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Initiator = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentWorkflowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowInstances_DocumentWorkflowSteps_CurrentStep~",
                        column: x => x.CurrentStepId,
                        principalTable: "DocumentWorkflowSteps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowInstances_DocumentWorkflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "DocumentWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowInstances_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentMetadataValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetadataFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentMetadataValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentMetadataValues_DocumentTemplateMetadataFields_Metad~",
                        column: x => x.MetadataFieldId,
                        principalTable: "DocumentTemplateMetadataFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentMetadataValues_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVersionApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalOrder = table.Column<int>(type: "integer", nullable: false),
                    Approver = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVersionApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentVersionApprovals_DocumentVersions_DocumentVersionId",
                        column: x => x.DocumentVersionId,
                        principalTable: "DocumentVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentWorkflowStepInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssigneeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsSkipped = table.Column<bool>(type: "boolean", nullable: false),
                    Assignee = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentWorkflowStepInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowStepInstances_DocumentWorkflowInstances_Wor~",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "DocumentWorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowStepInstances_DocumentWorkflowSteps_Workflo~",
                        column: x => x.WorkflowStepId,
                        principalTable: "DocumentWorkflowSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentWorkflowStepApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowStepInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalOrder = table.Column<int>(type: "integer", nullable: false),
                    Approver = table.Column<string>(type: "text", nullable: false),
                    DocumentWorkflowStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentWorkflowStepApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowStepApprovals_DocumentWorkflowStepInstances~",
                        column: x => x.WorkflowStepInstanceId,
                        principalTable: "DocumentWorkflowStepInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentWorkflowStepApprovals_DocumentWorkflowSteps_Documen~",
                        column: x => x.DocumentWorkflowStepId,
                        principalTable: "DocumentWorkflowSteps",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAccessControls_DocumentId",
                table: "DocumentAccessControls",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCategoreis_ParentCategoryId",
                table: "DocumentCategoreis",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDocumentCategory_DocumentsId",
                table: "DocumentDocumentCategory",
                column: "DocumentsId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDocumentTag_DocumentsId",
                table: "DocumentDocumentTag",
                column: "DocumentsId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMetadataValues_DocumentId",
                table: "DocumentMetadataValues",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMetadataValues_MetadataFieldId",
                table: "DocumentMetadataValues",
                column: "MetadataFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentTemplateId",
                table: "Documents",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplateMetadataFields_DocumentTemplateId",
                table: "DocumentTemplateMetadataFields",
                column: "DocumentTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_AssociatedWorkflowId",
                table: "DocumentTemplates",
                column: "AssociatedWorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersionApprovals_DocumentVersionId",
                table: "DocumentVersionApprovals",
                column: "DocumentVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_DocumentId",
                table: "DocumentVersions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowInstances_CurrentStepId",
                table: "DocumentWorkflowInstances",
                column: "CurrentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowInstances_DocumentId",
                table: "DocumentWorkflowInstances",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowInstances_WorkflowId",
                table: "DocumentWorkflowInstances",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowStepApprovals_DocumentWorkflowStepId",
                table: "DocumentWorkflowStepApprovals",
                column: "DocumentWorkflowStepId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowStepApprovals_WorkflowStepInstanceId",
                table: "DocumentWorkflowStepApprovals",
                column: "WorkflowStepInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowStepInstances_WorkflowInstanceId",
                table: "DocumentWorkflowStepInstances",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowStepInstances_WorkflowStepId",
                table: "DocumentWorkflowStepInstances",
                column: "WorkflowStepId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentWorkflowSteps_WorkflowId",
                table: "DocumentWorkflowSteps",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoHistories");

            migrationBuilder.DropTable(
                name: "DocumentAccessControls");

            migrationBuilder.DropTable(
                name: "DocumentDocumentCategory");

            migrationBuilder.DropTable(
                name: "DocumentDocumentTag");

            migrationBuilder.DropTable(
                name: "DocumentMetadataValues");

            migrationBuilder.DropTable(
                name: "DocumentVersionApprovals");

            migrationBuilder.DropTable(
                name: "DocumentWorkflowStepApprovals");

            migrationBuilder.DropTable(
                name: "DocumentCategoreis");

            migrationBuilder.DropTable(
                name: "DocumentTags");

            migrationBuilder.DropTable(
                name: "DocumentTemplateMetadataFields");

            migrationBuilder.DropTable(
                name: "DocumentVersions");

            migrationBuilder.DropTable(
                name: "DocumentWorkflowStepInstances");

            migrationBuilder.DropTable(
                name: "DocumentWorkflowInstances");

            migrationBuilder.DropTable(
                name: "DocumentWorkflowSteps");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "DocumentTemplates");

            migrationBuilder.DropTable(
                name: "DocumentWorkflows");
        }
    }
}
