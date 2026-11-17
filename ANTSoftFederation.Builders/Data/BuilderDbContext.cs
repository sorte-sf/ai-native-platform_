
using SorteFederation.EncryptEFCoreColumn;
using SorteFederation.Builders.Data.Entities;
using DefinitionStatus = SorteFederation.Builders.Data.Enums.DefinitionStatus;

namespace SorteFederation.Builders.Data;

public sealed class BuilderDbContext(DbContextOptions<BuilderDbContext> options,
    IEncryptionProvider provider,
    IHttpContextAccessor? accessor = null
    ) : DbContext(options)
{ 
    private bool IsHttpRequest => accessor?.HttpContext is not null;
    private bool IsSystemAdmin => accessor?.HttpContext?.User.IsInRole(BuilderRoleNames.SystemAdmin) == true;
    private string? CurrentUserId => accessor?.HttpContext is { } context ? ApplicationEditingPolicy.UserId(context.User) : null;
    private bool IsTenantAdmin => accessor?.HttpContext?.User.IsInRole(BuilderRoleNames.TenantAdmin) == true;
    private bool IsWorkspaceDesigner => ApplicationEditingPolicy.HasWorkspaceRole(accessor?.HttpContext?.User ?? new ClaimsPrincipal(), "BuilderAdmin", "BuilderEditor", "BuilderViewer");
    private Guid CurrentTenantId => Guid.TryParse(
        accessor?.HttpContext?.User.FindFirstValue("tenant_id")
        ?? accessor?.HttpContext?.User.FindFirstValue("company_id")
        ?? accessor?.HttpContext?.User.FindFirstValue("companyId"), out var id)
        ? id : Guid.Empty;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSmtpConfiguration> TenantSmtpConfigurations => Set<TenantSmtpConfiguration>();
    public DbSet<TenantTableMapping> TenantTableMappings => Set<TenantTableMapping>();
    public DbSet<TenantIntegrationConfiguration> TenantIntegrationConfigurations => Set<TenantIntegrationConfiguration>();

    public DbSet<BuilderApplication> Applications => Set<BuilderApplication>();
    public DbSet<BuilderApplicationUser> ApplicationUsers => Set<BuilderApplicationUser>();
    public DbSet<BuilderWorkspaceAccess> WorkspaceAccess => Set<BuilderWorkspaceAccess>();
    public DbSet<BuilderApplicationMembership> ApplicationMemberships => Set<BuilderApplicationMembership>();
    public DbSet<BuilderApplicationRole> ApplicationRoles => Set<BuilderApplicationRole>();
    public DbSet<BuilderUserGroup> UserGroups => Set<BuilderUserGroup>();
    public DbSet<BuilderUserGroupMembership> UserGroupMemberships => Set<BuilderUserGroupMembership>();
    public DbSet<BuilderGroupRoleAssignment> GroupRoleAssignments => Set<BuilderGroupRoleAssignment>();
    public DbSet<BuilderApplicationDeletionRequest> ApplicationDeletionRequests => Set<BuilderApplicationDeletionRequest>();
    public DbSet<BuilderApplicationMenuItem> ApplicationMenuItems => Set<BuilderApplicationMenuItem>();
    public DbSet<BuilderApplicationModule> ApplicationModules => Set<BuilderApplicationModule>();
    public DbSet<BuilderTemplate> Templates => Set<BuilderTemplate>();
    public DbSet<BuilderTemplateRelease> TemplateReleases => Set<BuilderTemplateRelease>();
    public DbSet<BuilderApplicationScreen> ApplicationScreens => Set<BuilderApplicationScreen>();
    public DbSet<BuilderApplicationVersion> ApplicationVersions => Set<BuilderApplicationVersion>();
    public DbSet<BuilderObject> Objects => Set<BuilderObject>();
    public DbSet<BuilderField> Fields => Set<BuilderField>();
    public DbSet<BuilderForm> Forms => Set<BuilderForm>();
    public DbSet<BuilderRecord> Records => Set<BuilderRecord>();
    public DbSet<BuilderRelationship> Relationships => Set<BuilderRelationship>();
    public DbSet<BuilderPermission> Permissions => Set<BuilderPermission>();
    public DbSet<BuilderPermissionGrant> PermissionGrants => Set<BuilderPermissionGrant>();
    public DbSet<BuilderWorkflow> Workflows => Set<BuilderWorkflow>();
    public DbSet<BuilderWorkflowApprovalPolicy> WorkflowApprovalPolicies => Set<BuilderWorkflowApprovalPolicy>();
    public DbSet<BuilderConnector> Connectors => Set<BuilderConnector>();
    public DbSet<BuilderRecordRelationship> RecordRelationships => Set<BuilderRecordRelationship>();
    public DbSet<BuilderWorkflowInstance> WorkflowInstances => Set<BuilderWorkflowInstance>();
    public DbSet<BuilderWorkflowTask> WorkflowTasks => Set<BuilderWorkflowTask>();
    public DbSet<BuilderWorkflowTransition> WorkflowTransitions => Set<BuilderWorkflowTransition>();
    public DbSet<BuilderReport> Reports => Set<BuilderReport>();
    public DbSet<BuilderEmailTemplate> EmailTemplates => Set<BuilderEmailTemplate>();
    public DbSet<BuilderDashboard> Dashboards => Set<BuilderDashboard>();
    public DbSet<BuilderDashboardWidget> DashboardWidgets => Set<BuilderDashboardWidget>();
    public DbSet<BuilderConnectorCredential> ConnectorCredentials => Set<BuilderConnectorCredential>();
    public DbSet<BuilderAuditEvent> AuditEvents => Set<BuilderAuditEvent>();
    public DbSet<BuilderFeatureFlag> FeatureFlags => Set<BuilderFeatureFlag>();
    public DbSet<BuilderScheduledJob> ScheduledJobs => Set<BuilderScheduledJob>();
    public DbSet<BuilderKnowledgeDocument> KnowledgeDocuments => Set<BuilderKnowledgeDocument>();
    public DbSet<BuilderRule> Rules => Set<BuilderRule>();
    public DbSet<BuilderNotification> Notifications => Set<BuilderNotification>();
    public DbSet<BuilderPlatformEvent> PlatformEvents => Set<BuilderPlatformEvent>();
    public DbSet<BuilderAiTenantPolicy> AiTenantPolicies => Set<BuilderAiTenantPolicy>();
    public DbSet<BuilderAiUserModel> AiUserModels => Set<BuilderAiUserModel>();
    public DbSet<BuilderAiUsage> AiUsage => Set<BuilderAiUsage>();
    public DbSet<BuilderAiUsagePeriod> AiUsagePeriods => Set<BuilderAiUsagePeriod>();
    public DbSet<BuilderUserFavouriteLink> UserFavouriteLinks => Set<BuilderUserFavouriteLink>();

    /*public DbSet<TimeTickerEntity> TimeTickers { get; set; }
    public DbSet<CronTickerEntity> CronTickers { get; set; }
    public DbSet<CronTickerOccurrenceEntity<CronTickerEntity>> CronTickerOccurrences { get; set; }*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        /*modelBuilder.ApplyConfiguration<TimeTickerEntity>(new TimeTickerConfigurations<TimeTickerEntity>("ticker"));
        modelBuilder.ApplyConfiguration<CronTickerEntity>(new CronTickerConfigurations<CronTickerEntity>("ticker"));
        modelBuilder.ApplyConfiguration(new CronTickerOccurrenceConfigurations<CronTickerEntity>("ticker"));*/

        modelBuilder.HasDefaultSchema("builders");
        modelBuilder.Entity<Tenant>().ToTable("Tenant").HasKey(x => x.TenantId);
        modelBuilder.Entity<Tenant>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Tenant>().Property(x => x.SubscriptionEndsOn).HasColumnType("date");
        modelBuilder.Entity<Tenant>().Property(x => x.SubscriptionStartsOn).HasColumnType("date");
        modelBuilder.Entity<TenantSmtpConfiguration>().ToTable("TenantSmtpConfiguration");
        modelBuilder.Entity<TenantSmtpConfiguration>().HasIndex(x => x.TenantId).IsUnique();
        modelBuilder.Entity<TenantSmtpConfiguration>().HasOne(x => x.Tenant).WithOne(x => x.TenantSmtpConfiguration)
            .HasForeignKey<TenantSmtpConfiguration>(x => x.TenantId).HasPrincipalKey<Tenant>(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TenantIntegrationConfiguration>().ToTable("TenantIntegrationConfiguration");
        modelBuilder.Entity<TenantIntegrationConfiguration>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Name }).IsUnique();
        modelBuilder.Entity<TenantIntegrationConfiguration>().HasOne(x => x.Tenant).WithMany(x => x.IntegrationConfigurations)
            .HasForeignKey(x => x.TenantId).HasPrincipalKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TenantIntegrationConfiguration>().HasOne(x => x.Application).WithMany()
            .HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TenantTableMapping>().ToTable("TenantTableMapping");
        modelBuilder.Entity<TenantTableMapping>().HasOne(x => x.TenantIntegrationConfiguration).WithMany(x => x.TenantTableMappings)
            .HasForeignKey(x => x.TenantIntegrationConfigurationId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TenantTableMapping>().HasIndex(x => new { x.TenantIntegrationConfigurationId, x.SourceFieldName, x.DestinationFieldName }).IsUnique();
        modelBuilder.Entity<TenantSmtpConfiguration>().Property(x => x.Host).HasMaxLength(255);
        modelBuilder.Entity<TenantSmtpConfiguration>().Property(x => x.From).HasMaxLength(320);
        modelBuilder.Entity<TenantSmtpConfiguration>().Property(x => x.DisplayName).HasMaxLength(160);
        modelBuilder.Entity<TenantSmtpConfiguration>().Property(x => x.UserName).HasMaxLength(320);
        modelBuilder.Entity<TenantIntegrationConfiguration>().Property(x => x.Name).HasMaxLength(160);
        modelBuilder.Entity<TenantIntegrationConfiguration>().Property(x => x.UserName).HasMaxLength(320);
        modelBuilder.Entity<TenantIntegrationConfiguration>().Property(x => x.DatabaseName).HasMaxLength(256);
        modelBuilder.Entity<TenantTableMapping>().Property(x => x.SourceFieldName).HasMaxLength(160);
        modelBuilder.Entity<TenantTableMapping>().Property(x => x.DestinationFieldName).HasMaxLength(160);
        modelBuilder.Entity<TenantTableMapping>().Property(x => x.SourceFieldDescription).HasMaxLength(1000);
        modelBuilder.Entity<BuilderApplication>().Property(x => x.CreatedByUserId).HasMaxLength(200);
        modelBuilder.Entity<BuilderApplicationUser>().ToTable("ApplicationUsers");
        modelBuilder.Entity<BuilderApplicationUser>().Property(x => x.Email).HasMaxLength(320).IsRequired();
        modelBuilder.Entity<BuilderApplicationUser>().Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
        modelBuilder.Entity<BuilderApplicationUser>().Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<BuilderApplicationUser>().Property(x => x.PasswordHash).HasMaxLength(1000).IsRequired();
        modelBuilder.Entity<BuilderApplicationUser>().HasIndex(x => new { x.TenantId, x.NormalizedEmail }).IsUnique();
        modelBuilder.Entity<BuilderApplicationMembership>().ToTable("ApplicationMemberships");
        modelBuilder.Entity<BuilderApplicationMembership>().Property(x => x.Role).HasMaxLength(80).IsRequired();
        modelBuilder.Entity<BuilderApplicationMembership>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.UserId }).IsUnique();
        modelBuilder.Entity<BuilderApplicationUser>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<BuilderApplicationMembership>().HasOne<BuilderApplication>().WithMany()
            .HasForeignKey(x => new { x.TenantId, x.ApplicationId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderApplicationMembership>().HasOne(x => x.User).WithMany(x => x.Memberships)
            .HasForeignKey(x => new { x.TenantId, x.UserId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderWorkspaceAccess>().ToTable("BuilderWorkspaceAccess");
        modelBuilder.Entity<BuilderWorkspaceAccess>().Property(x => x.Role).HasMaxLength(32).IsRequired();
        modelBuilder.Entity<BuilderWorkspaceAccess>().HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
        modelBuilder.Entity<BuilderWorkspaceAccess>().HasOne(x => x.User).WithMany()
            .HasForeignKey(x => new { x.TenantId, x.UserId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderApplicationRole>().ToTable("ApplicationRoles");
        modelBuilder.Entity<BuilderApplicationRole>().Property(x => x.Key).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<BuilderApplicationRole>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<BuilderApplicationRole>().Property(x => x.Description).HasMaxLength(1000);
        modelBuilder.Entity<BuilderApplicationRole>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<BuilderApplicationRole>().HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderUserGroup>().ToTable("UserGroups");
        modelBuilder.Entity<BuilderUserGroup>().Property(x => x.Key).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<BuilderUserGroup>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<BuilderUserGroup>().Property(x => x.Description).HasMaxLength(1000);
        modelBuilder.Entity<BuilderUserGroup>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<BuilderUserGroup>().HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderUserGroupMembership>().ToTable("UserGroupMemberships");
        modelBuilder.Entity<BuilderUserGroupMembership>().HasIndex(x => new { x.TenantId, x.GroupId, x.UserId }).IsUnique();
        modelBuilder.Entity<BuilderUserGroupMembership>().HasOne(x => x.User).WithMany()
            .HasForeignKey(x => new { x.TenantId, x.UserId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderUserGroupMembership>().HasOne(x => x.Group).WithMany(x => x.Members)
            .HasForeignKey(x => new { x.TenantId, x.GroupId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderGroupRoleAssignment>().ToTable("GroupRoleAssignments");
        modelBuilder.Entity<BuilderGroupRoleAssignment>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.GroupId, x.RoleId }).IsUnique();
        modelBuilder.Entity<BuilderGroupRoleAssignment>().HasOne(x => x.Group).WithMany(x => x.RoleAssignments)
            .HasForeignKey(x => new { x.TenantId, x.GroupId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderGroupRoleAssignment>().HasOne(x => x.Role).WithMany()
            .HasForeignKey(x => new { x.TenantId, x.RoleId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderGroupRoleAssignment>().HasOne<BuilderApplication>().WithMany()
            .HasForeignKey(x => new { x.TenantId, x.ApplicationId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderApplicationDeletionRequest>().Property(x => x.RequestedByUserId).HasMaxLength(200);
        modelBuilder.Entity<BuilderApplicationDeletionRequest>().Property(x => x.ReviewedByUserId).HasMaxLength(200);
        modelBuilder.Entity<BuilderApplicationDeletionRequest>().Property(x => x.Reason).HasMaxLength(2000);
        modelBuilder.Entity<BuilderApplicationDeletionRequest>().Property(x => x.ReviewReason).HasMaxLength(2000);
        modelBuilder.Entity<BuilderApplicationDeletionRequest>().HasIndex(x => new { x.ApplicationId, x.Status });
        modelBuilder.Entity<BuilderApplication>().HasIndex(x => new { x.TenantId, x.CreatedByUserId });
        modelBuilder.Entity<BuilderApplication>()
                    .HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderApplication>()
                    .Property(e => e.Slugs).HasConversion(v => string.Join(",", v), v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

        modelBuilder.Entity<BuilderApplicationMenuItem>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderApplicationModule>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderApplicationModule>().HasAlternateKey(x => new { x.TenantId, x.ApplicationId, x.Id });
        modelBuilder.Entity<BuilderApplicationScreen>().HasAlternateKey(x => new { x.TenantId, x.ApplicationId, x.Id });
        modelBuilder.Entity<BuilderObject>().HasAlternateKey(x => new { x.TenantId, x.ApplicationId, x.Id });

        modelBuilder.Entity<BuilderForm>().HasAlternateKey(x => new { x.TenantId, x.ObjectId, x.Id });

        modelBuilder.Entity<BuilderApplicationModule>().HasOne<BuilderApplicationScreen>().WithMany()
            .HasForeignKey(x => new { x.TenantId, x.ApplicationId, x.ScreenId })
            .HasPrincipalKey(x => new { x.TenantId, x.ApplicationId, x.Id }).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BuilderApplicationModule>().HasOne<BuilderObject>().WithMany()
            .HasForeignKey(x => new { x.TenantId, x.ApplicationId, x.ObjectId })
            .HasPrincipalKey(x => new { x.TenantId, x.ApplicationId, x.Id }).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BuilderApplicationModule>().HasOne<BuilderForm>().WithMany()
            .HasForeignKey(x => new { x.TenantId, x.ObjectId, x.FormId })
            .HasPrincipalKey(x => new { x.TenantId, x.ObjectId, x.Id }).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BuilderApplication>().HasMany(x => x.Modules).WithOne().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderApplication>().HasMany(x => x.MenuItems).WithOne().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BuilderApplicationMenuItem>().HasOne<BuilderApplicationModule>().WithMany()
            .HasForeignKey(x => new { x.TenantId, x.ApplicationId, x.ModuleId })
            .HasPrincipalKey(x => new { x.TenantId, x.ApplicationId, x.Id }).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BuilderApplicationMenuItem>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.ModuleId });
        modelBuilder.Entity<BuilderApplicationMenuItem>().HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderTemplate>().Ignore(x => x.TenantId);
        modelBuilder.Entity<BuilderTemplate>().HasIndex(x => x.Name);
        modelBuilder.Entity<BuilderTemplate>().Property(x => x.Category).HasMaxLength(64);
        modelBuilder.Entity<BuilderTemplate>().Property(x => x.BlueprintVersion).HasMaxLength(64);
        modelBuilder.Entity<BuilderTemplate>().Property(x => x.PreviewImageUrl).HasMaxLength(1000);
        modelBuilder.Entity<BuilderTemplate>().HasIndex(x => x.Category);
        modelBuilder.Entity<BuilderTemplate>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<BuilderTemplate>().Property(x => x.ManifestJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderTemplate>().Property(x => x.ThemeJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderApplication>().Property(x => x.ThemeJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderApplicationScreen>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderApplicationScreen>().Property(x => x.DefinitionJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderApplication>().HasMany(x => x.Screens).WithOne().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderApplication>().HasOne(x => x.Template).WithMany(x => x.Applications).HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<BuilderTemplate>().Property(x => x.HtmlContent).HasColumnType("text");
        modelBuilder.Entity<BuilderTemplate>().Property(x => x.ScssContent).HasColumnType("text");
        modelBuilder.Entity<BuilderTemplateRelease>().ToTable("TemplateReleases");
        modelBuilder.Entity<BuilderTemplateRelease>().HasIndex(x => new { x.TemplateId, x.ReleaseVersion }).IsUnique();
        modelBuilder.Entity<BuilderTemplateRelease>().Property(x => x.ReleaseVersion).HasMaxLength(64);
        modelBuilder.Entity<BuilderTemplateRelease>().Property(x => x.ManifestJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderTemplateRelease>().Property(x => x.ThemeJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderTemplateRelease>().Property(x => x.HtmlContent).HasColumnType("text");
        modelBuilder.Entity<BuilderTemplateRelease>().Property(x => x.ScssContent).HasColumnType("text");
        modelBuilder.Entity<BuilderApplication>().Property(x => x.AppliedTemplateVersion).HasMaxLength(64);
        modelBuilder.Entity<BuilderTemplateRelease>().HasOne(x => x.Template).WithMany(x => x.Releases)
            .HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderApplication>().HasOne<BuilderTemplateRelease>()
            .WithMany().HasForeignKey(x => x.TemplateReleaseId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<BuilderApplicationVersion>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.VersionNumber }).IsUnique();
        modelBuilder.Entity<BuilderApplicationVersion>().Property(x => x.SnapshotJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderApplicationVersion>().Property(x => x.ValidationErrorsJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderApplication>().HasMany(x => x.Versions).WithOne().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderObject>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderField>().HasIndex(x => new { x.TenantId, x.ObjectId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderForm>().HasIndex(x => new { x.TenantId, x.ObjectId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderRecord>().Property(x => x.DataJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderField>().Property(x => x.ConfigurationJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderForm>().Property(x => x.LayoutJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderWorkflow>().Property(x => x.DefinitionJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderWorkflowApprovalPolicy>().ToTable("WorkflowApprovalPolicies");
        modelBuilder.Entity<BuilderWorkflowApprovalPolicy>().Property(x => x.Key).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<BuilderWorkflowApprovalPolicy>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<BuilderWorkflowApprovalPolicy>().Property(x => x.StepsJson).HasColumnType("jsonb").IsRequired();
        modelBuilder.Entity<BuilderWorkflowApprovalPolicy>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderConnector>().Property(x => x.ConfigurationJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderRelationship>().HasIndex(x => new { x.TenantId, x.SourceObjectId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderPermission>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderPermissionGrant>().HasIndex(x => new { x.TenantId, x.PermissionId, x.SubjectType, x.SubjectId }).IsUnique();
        modelBuilder.Entity<BuilderPermissionGrant>().HasOne(x => x.Permission).WithMany(x => x.Grants).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderWorkflow>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderWorkflow>().HasIndex(x => new { x.Status, x.TenantId });
        modelBuilder.Entity<BuilderConnector>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderRecordRelationship>().HasIndex(x => new { x.TenantId, x.RelationshipId, x.SourceRecordId, x.TargetRecordId }).IsUnique();
        modelBuilder.Entity<BuilderWorkflowInstance>().HasIndex(x => new { x.TenantId, x.WorkflowId, x.RecordId }).IsUnique();
        modelBuilder.Entity<BuilderWorkflowInstance>().Property(x => x.CurrentState).HasMaxLength(120);
        modelBuilder.Entity<BuilderWorkflowInstance>().HasMany(x => x.Tasks).WithOne(x => x.WorkflowInstance).HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderWorkflow>().HasMany(x => x.Transitions).WithOne().HasForeignKey(x => x.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderWorkflowTransition>().Property(x => x.ConditionJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderReport>().Property(x => x.DefinitionJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderEmailTemplate>().Property(x => x.DefinitionJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderEmailTemplate>().Property(x => x.Key).HasMaxLength(80);
        modelBuilder.Entity<BuilderEmailTemplate>().Property(x => x.Name).HasMaxLength(120);
        modelBuilder.Entity<BuilderEmailTemplate>().Property(x => x.Subject).HasMaxLength(250);
        modelBuilder.Entity<BuilderEmailTemplate>().Property(x => x.Language).HasMaxLength(16);
        modelBuilder.Entity<BuilderEmailTemplate>().Property(x => x.TemplateType).HasMaxLength(60);
        modelBuilder.Entity<BuilderEmailTemplate>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderApplication>().HasMany<BuilderEmailTemplate>().WithOne().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderDashboard>().Property(x => x.LayoutJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderDashboardWidget>().Property(x => x.ConfigurationJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderDashboard>().HasMany(x => x.Widgets).WithOne().HasForeignKey(x => x.DashboardId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderDashboardWidget>().HasIndex(x => new { x.TenantId, x.DashboardId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderAuditEvent>().Property(x => x.DataJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderAuditEvent>().HasIndex(x => new { x.TenantId, x.CreatedAt });
        modelBuilder.Entity<BuilderReport>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderFeatureFlag>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderScheduledJob>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderFeatureFlag>().Property(x => x.RulesJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderScheduledJob>().Property(x => x.ActionJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderScheduledJob>().Property(x => x.TimeZoneId).HasDefaultValue("Europe/Istanbul");
        modelBuilder.Entity<BuilderScheduledJob>().Property(x => x.ScheduleType).HasDefaultValue("recurring");
        modelBuilder.Entity<BuilderKnowledgeDocument>().Property(x => x.MetadataJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderKnowledgeDocument>().HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderRule>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderRule>().Property(x => x.DefinitionJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderNotification>().HasIndex(x => new { x.TenantId, x.RecipientId, x.ReadAt });
        modelBuilder.Entity<BuilderNotification>().Property(x => x.RecipientId).HasMaxLength(200);
        modelBuilder.Entity<BuilderPlatformEvent>().Property(x => x.PayloadJson).HasColumnType("jsonb");
        modelBuilder.Entity<BuilderPlatformEvent>().HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
        modelBuilder.Entity<BuilderAiTenantPolicy>().HasIndex(x => x.TenantId).IsUnique();
        modelBuilder.Entity<BuilderAiTenantPolicy>().Property(x => x.Mode).HasMaxLength(32);
        modelBuilder.Entity<BuilderAiTenantPolicy>().Property(x => x.Provider).HasMaxLength(80);
        modelBuilder.Entity<BuilderAiTenantPolicy>().Property(x => x.Endpoint).HasMaxLength(500);
        modelBuilder.Entity<BuilderAiTenantPolicy>().Property(x => x.Model).HasMaxLength(160);
        modelBuilder.Entity<BuilderAiTenantPolicy>().Property(x => x.ApiKeyReference).HasMaxLength(300);
        modelBuilder.Entity<BuilderAiTenantPolicy>().Property(x => x.PersonalApiKey).HasMaxLength(4096);
        modelBuilder.Entity<BuilderAiTenantPolicy>().Property(x => x.AuthScheme).HasMaxLength(32);
        modelBuilder.Entity<BuilderAiUserModel>().ToTable("AiUserModels");
        modelBuilder.Entity<BuilderAiUserModel>().HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
        modelBuilder.Entity<BuilderAiUserModel>().Property(x => x.UserId).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<BuilderAiUserModel>().Property(x => x.Provider).HasMaxLength(80).IsRequired();
        modelBuilder.Entity<BuilderAiUserModel>().Property(x => x.Endpoint).HasMaxLength(500).IsRequired();
        modelBuilder.Entity<BuilderAiUserModel>().Property(x => x.Model).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<BuilderAiUserModel>().Property(x => x.ApiKey).HasMaxLength(4096).IsRequired();
        modelBuilder.Entity<BuilderAiUserModel>().Property(x => x.AuthScheme).HasMaxLength(32).IsRequired();
        modelBuilder.Entity<BuilderAiUsage>().HasIndex(x => new { x.TenantId, x.CreatedAt });
        modelBuilder.Entity<BuilderAiUsage>().HasIndex(x => new { x.TenantId, x.Feature, x.CreatedAt });
        modelBuilder.Entity<BuilderAiUsage>().HasIndex(x => x.RequestId).IsUnique();
        modelBuilder.Entity<BuilderAiUsage>().Property(x => x.Feature).HasMaxLength(80);
        modelBuilder.Entity<BuilderAiUsage>().Property(x => x.Provider).HasMaxLength(80);
        modelBuilder.Entity<BuilderAiUsage>().Property(x => x.Model).HasMaxLength(160);
        modelBuilder.Entity<BuilderAiUsage>().Property(x => x.Outcome).HasMaxLength(32);
        modelBuilder.Entity<BuilderAiUsage>().Property(x => x.FailureCode).HasMaxLength(80);
        modelBuilder.Entity<BuilderAiUsage>().Property(x => x.UserId).HasMaxLength(200);
        modelBuilder.Entity<BuilderAiUsagePeriod>().HasIndex(x => new { x.TenantId, x.PeriodStart }).IsUnique();
        modelBuilder.Entity<BuilderUserFavouriteLink>().ToTable("UserFavouriteLinks");
        modelBuilder.Entity<BuilderUserFavouriteLink>().Property(x => x.UserId).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<BuilderUserFavouriteLink>().Property(x => x.Name).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<BuilderUserFavouriteLink>().Property(x => x.PublishedVersion).HasMaxLength(64).IsRequired();
        modelBuilder.Entity<BuilderUserFavouriteLink>().Property(x => x.SpecificationJson).HasColumnType("jsonb").IsRequired();
        modelBuilder.Entity<BuilderUserFavouriteLink>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.UserId, x.Name }).IsUnique();
        modelBuilder.Entity<BuilderUserFavouriteLink>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.UserId });
        modelBuilder.Entity<BuilderDashboard>().HasIndex(x => new { x.TenantId, x.ApplicationId, x.Key }).IsUnique();
        modelBuilder.Entity<BuilderConnectorCredential>().HasIndex(x => new { x.TenantId, x.ConnectorId, x.Scheme }).IsUnique();
        modelBuilder.Entity<BuilderApplication>().HasMany(x => x.Objects).WithOne().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderObject>().HasMany(x => x.Fields).WithOne().HasForeignKey(x => x.ObjectId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BuilderObject>().HasMany(x => x.Forms).WithOne().HasForeignKey(x => x.ObjectId).OnDelete(DeleteBehavior.Cascade);

        foreach (var entity in modelBuilder.Model.GetEntityTypes().ToArray())
        {
            if (!typeof(BuilderEntity).IsAssignableFrom(entity.ClrType) || entity.FindProperty(nameof(BuilderEntity.TenantId)) is null)
                continue;

            var method = typeof(BuilderDbContext).GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            method.MakeGenericMethod(entity.ClrType).Invoke(this, [modelBuilder]);

            if (entity.FindProperty("ApplicationId")?.ClrType == typeof(Guid))
                typeof(BuilderDbContext).GetMethod(nameof(ApplyApplicationReadFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entity.ClrType).Invoke(this, [modelBuilder]);
        }
        modelBuilder.Entity<BuilderApplication>().HasQueryFilter(entity =>
            (!IsHttpRequest || IsSystemAdmin || (CurrentTenantId != Guid.Empty && entity.TenantId == CurrentTenantId &&
            (IsTenantAdmin || IsWorkspaceDesigner || entity.Status == DefinitionStatus.Published ||
             (CurrentUserId != null && entity.CreatedByUserId == CurrentUserId)))) && entity.RecordState == RecordState.Active);

        modelBuilder.Entity<BuilderField>().HasQueryFilter(entity => !IsHttpRequest || IsSystemAdmin ||
            (entity.TenantId == CurrentTenantId && Objects.Any(obj => obj.Id == entity.ObjectId && obj.TenantId == entity.TenantId)));

        modelBuilder.Entity<BuilderForm>().HasQueryFilter(entity => !IsHttpRequest || IsSystemAdmin ||
            (entity.TenantId == CurrentTenantId && Objects.Any(obj => obj.Id == entity.ObjectId && obj.TenantId == entity.TenantId)));

        modelBuilder.UseEncryption(provider);
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BuilderEntity =>
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity =>
            !IsHttpRequest || IsSystemAdmin || (CurrentTenantId != Guid.Empty && entity.TenantId == CurrentTenantId));

    private void ApplyApplicationReadFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BuilderEntity =>
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => !IsHttpRequest || IsSystemAdmin ||
            (CurrentTenantId != Guid.Empty && entity.TenantId == CurrentTenantId &&
             Applications.Any(application => application.Id == EF.Property<Guid>(entity, "ApplicationId") && application.TenantId == entity.TenantId)));

    public override int SaveChanges()
        => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureTenantWrites();
        EnsureApplicationWritesAsync(CancellationToken.None).GetAwaiter().GetResult();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(true, cancellationToken);

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnsureTenantWrites();
        await EnsureApplicationWritesAsync(cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private async Task EnsureApplicationWritesAsync(CancellationToken token)
    {
        if (!IsHttpRequest) return; // Trusted startup/background operations.
        var user = accessor!.HttpContext!.User;
        var changed = ChangeTracker.Entries<BuilderEntity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToArray();
        var addedApps = changed.Where(entry => entry.Entity is BuilderApplication && entry.State == EntityState.Added).ToArray();
        foreach (var entry in addedApps)
            ((BuilderApplication)entry.Entity).CreatedByUserId = CurrentUserId
                ?? throw new UnauthorizedAccessException("An authenticated creator is required.");
        foreach (var entry in changed.Where(entry => entry.Entity is BuilderApplication && entry.State != EntityState.Added))
            if (entry.Property(nameof(BuilderApplication.CreatedByUserId)).IsModified)
                throw new UnauthorizedAccessException("Application ownership cannot be changed.");
        if (IsSystemAdmin || IsTenantAdmin) return; // Tenant writes were checked first.
        var appIds = new HashSet<Guid>();
        var objectIds = new HashSet<Guid>();
        var dashboardIds = new HashSet<Guid>();
        var connectorIds = new HashSet<Guid>();
        var permissionIds = new HashSet<Guid>();
        var workflowIds = new HashSet<Guid>();
        foreach (var entry in changed)
        {
            if (entry.Entity is BuilderApplication app && entry.State != EntityState.Added) appIds.Add(app.Id);
            // Runtime records, tickets, notifications and workflow instances are
            // governed by their runtime permissions, not design ownership.
            if (entry.Entity is BuilderObject or BuilderApplicationScreen or BuilderApplicationModule or BuilderApplicationMenuItem
                or BuilderApplicationVersion or BuilderWorkflow or BuilderConnector or BuilderPermission or BuilderReport
                or BuilderDashboard or BuilderFeatureFlag or BuilderScheduledJob or BuilderRule or BuilderEmailTemplate)
            {
                appIds.Add((Guid)entry.Property("ApplicationId").CurrentValue!);
                if (entry.State != EntityState.Added) appIds.Add((Guid)entry.Property("ApplicationId").OriginalValue!);
            }
            foreach (var values in entry.State == EntityState.Added ? new[] { entry.CurrentValues } : new[] { entry.CurrentValues, entry.OriginalValues })
                switch (entry.Entity)
                {
                    case BuilderField or BuilderForm: objectIds.Add(values.GetValue<Guid>("ObjectId")); break;
                    case BuilderRelationship: objectIds.Add(values.GetValue<Guid>("SourceObjectId")); objectIds.Add(values.GetValue<Guid>("TargetObjectId")); break;
                    case BuilderDashboardWidget: dashboardIds.Add(values.GetValue<Guid>("DashboardId")); break;
                    case BuilderConnectorCredential: connectorIds.Add(values.GetValue<Guid>("ConnectorId")); break;
                    case BuilderPermissionGrant: permissionIds.Add(values.GetValue<Guid>("PermissionId")); break;
                    case BuilderWorkflowTransition: workflowIds.Add(values.GetValue<Guid>("WorkflowId")); break;
                }
        }
        // Resolve new parents from tracking and persisted parents in bounded set queries.
        foreach (var obj in ChangeTracker.Entries<BuilderObject>().Where(e => objectIds.Contains(e.Entity.Id))) appIds.Add(obj.Entity.ApplicationId);
        if (objectIds.Count > 0)
        {
            var parents = await Objects.Where(x => objectIds.Contains(x.Id)).Select(x => new { x.Id, x.ApplicationId }).ToListAsync(token);
            var tracked = ChangeTracker.Entries<BuilderObject>().Where(e => objectIds.Contains(e.Entity.Id)).Select(e => e.Entity).ToArray();
            if (parents.Select(x => x.Id).Concat(tracked.Select(x => x.Id)).Distinct().Count() != objectIds.Count) throw new UnauthorizedAccessException("Data object is outside the authenticated tenant.");
            appIds.UnionWith(parents.Select(x => x.ApplicationId));
        }
        if (dashboardIds.Count > 0) appIds.UnionWith(await ResolveMetadataParents<BuilderDashboard>(dashboardIds, token));
        if (connectorIds.Count > 0) appIds.UnionWith(await ResolveMetadataParents<BuilderConnector>(connectorIds, token));
        if (permissionIds.Count > 0) appIds.UnionWith(await ResolveMetadataParents<BuilderPermission>(permissionIds, token));
        if (workflowIds.Count > 0) appIds.UnionWith(await ResolveMetadataParents<BuilderWorkflow>(workflowIds, token));
        foreach (var entry in addedApps) appIds.Remove(entry.Entity.Id);
        if (appIds.Count == 0) return;
        var applications = await Applications.AsNoTracking().Where(x => appIds.Contains(x.Id)).ToListAsync(token);
        if (applications.Count != appIds.Count || applications.Any(app => !ApplicationEditingPolicy.CanEdit(user, app)))
            throw new UnauthorizedAccessException("Only the creator, SystemAdmin or TenantAdmin can modify this application.");
    }

    private async Task<Guid[]> ResolveMetadataParents<TEntity>(HashSet<Guid> ids, CancellationToken token) where TEntity : BuilderEntity
    {
        var parents = await Set<TEntity>().Where(entity => ids.Contains(entity.Id))
            .Select(entity => new { entity.Id, ApplicationId = EF.Property<Guid>(entity, "ApplicationId") }).ToListAsync(token);
        var tracked = ChangeTracker.Entries<TEntity>().Where(entry => ids.Contains(entry.Entity.Id)).ToArray();
        if (parents.Select(parent => parent.Id).Concat(tracked.Select(entry => entry.Entity.Id)).Distinct().Count() != ids.Count)
            throw new UnauthorizedAccessException("Metadata parent is outside the authenticated application.");
        return parents.Select(parent => parent.ApplicationId).Concat(tracked.Select(entry => (Guid)entry.Property("ApplicationId").CurrentValue!)).Distinct().ToArray();
    }

    private void EnsureTenantWrites()
    {
        if (!IsHttpRequest || IsSystemAdmin) return;
        if (ChangeTracker.Entries<BuilderEntity>().Any(entry =>
            (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted) &&
            entry.Metadata.FindProperty(nameof(BuilderEntity.TenantId)) is not null &&
            (CurrentTenantId == Guid.Empty || entry.Entity.TenantId != CurrentTenantId ||
             entry.State != EntityState.Added && entry.OriginalValues.GetValue<Guid>(nameof(BuilderEntity.TenantId)) != CurrentTenantId)))
            throw new UnauthorizedAccessException("Builders changes must belong to the authenticated tenant.");
    }
}
