 
using SorteFederation.GlobalEnums;
using DefinitionStatus = SorteFederation.Builders.Data.Enums.DefinitionStatus;

namespace SorteFederation.Builders.Endpoints;

public sealed record SetApplicationOperationalState(Guid TenantId, bool IsActive);
public sealed record ApplyTemplateUpdateRequest(Guid TenantId, Guid ReleaseId);

public sealed class ApplicationsEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapApplicationsEndpoints(endpoints);

public static IEndpointRouteBuilder MapApplicationsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/applications/{applicationId:guid}/drafts", async (
            Guid applicationId, ClaimsPrincipal user, BuilderDbContext db, PublishedApplicationDraftService drafts, CancellationToken token) =>
        {
            var app = await db.Applications.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(x => x.Id == applicationId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (await db.ApplicationDeletionRequests.AnyAsync(x => x.ApplicationId == app.Id && x.Status == ApplicationDeletionRequestStatus.Pending, token))
                return Results.Conflict(new { message = "This application has a pending deletion request and cannot be copied to a draft." });
            try { return Results.Ok(await drafts.CreateDraftAsync(applicationId, ApplicationEditingPolicy.UserId(user), token)); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
            catch (JsonException ex) { return Results.Conflict(new { message = $"The published snapshot is invalid: {ex.Message}" }); }
        }).RequireAuthorization().WithName("CreatePublishedApplicationDraft").WithTags("Applications");

        endpoints.MapGet("/applications/{applicationId:guid}/template-updates", async (
            Guid applicationId, ClaimsPrincipal user, BuilderDbContext db, TemplateUpdateService updates, CancellationToken token) =>
        {
            var tenantClaim = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId");
            var isSystemAdmin = user.IsInRole(BuilderRoleNames.SystemAdmin);
            var tenantId = Guid.Empty;
            if (!isSystemAdmin && !Guid.TryParse(tenantClaim, out tenantId)) return Results.Forbid();

            var app = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == applicationId && (isSystemAdmin || x.TenantId == tenantId), token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            return Results.Ok(await updates.ReviewAsync(applicationId, token));
        }).RequireAuthorization().WithName("GetApplicationTemplateUpdates").WithTags("Applications");

        endpoints.MapPost("/applications/{applicationId:guid}/template-updates/apply", async (
            Guid applicationId, ApplyTemplateUpdateRequest body, ClaimsPrincipal user, BuilderDbContext db,
            TemplateUpdateService updates, CancellationToken token) =>
        {
            var tenantClaim = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId");
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) &&
                (!Guid.TryParse(tenantClaim, out var claimedTenant) || claimedTenant != body.TenantId)) return Results.Forbid();
            var app = await db.Applications.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == body.TenantId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            try { return Results.Ok(await updates.ApplyAsync(applicationId, body.TenantId, body.ReleaseId, ApplicationEditingPolicy.UserId(user), token)); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { message = ex.Message }); }
            catch (ArgumentException ex) { return Results.Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
        }).RequireAuthorization().WithName("ApplyApplicationTemplateUpdate").WithTags("Applications");

        endpoints.MapGet("/applications/{applicationId:guid}/versions", async (
            Guid applicationId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            var isSystemAdmin = user.IsInRole(BuilderRoleNames.SystemAdmin);
            var tenantClaim = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId");
            var tenantId = Guid.Empty;
            if (!isSystemAdmin && !Guid.TryParse(tenantClaim, out tenantId)) return Results.Forbid();

            var app = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == applicationId && (isSystemAdmin || x.TenantId == tenantId), token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();

            var versions = await db.ApplicationVersions.AsNoTracking()
                .Where(x => x.ApplicationId == app.Id && x.TenantId == app.TenantId && x.State == GovernanceState.Published)
                .OrderByDescending(x => x.PublishedAt)
                .Select(x => new { x.Id, x.VersionNumber, x.PublishedAt, x.SnapshotJson })
                .ToListAsync(token);

            var history = versions.Select(version =>
            {
                JsonElement? templateSource = null;
                try
                {
                    using var snapshot = JsonDocument.Parse(version.SnapshotJson);
                    if (snapshot.RootElement.TryGetProperty("TemplateSource", out var source) && source.ValueKind == JsonValueKind.Object)
                        templateSource = source.Clone();
                }
                catch (JsonException) { /* Keep legacy or malformed snapshots visible in history. */ }

                return new { version.Id, version.VersionNumber, version.PublishedAt, TemplateSource = templateSource };
            });
            return Results.Ok(history);
        }).RequireAuthorization().WithName("ListBuilderApplicationVersions").WithTags("Applications");

        endpoints.MapGet("/applications", async (Guid? tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            var isSystemAdmin = user.IsInRole(BuilderRoleNames.SystemAdmin);
            var isTenantAdmin = user.IsInRole(BuilderRoleNames.TenantAdmin);
            var isWorkspace = user.HasClaim("builder_workspace_user", "true") && ApplicationEditingPolicy.HasWorkspaceRole(user, "BuilderAdmin", "BuilderEditor", "BuilderViewer");
            var currentUserId = ApplicationEditingPolicy.UserId(user);

            if (!isSystemAdmin && tenantId is null) return Results.BadRequest("tenantId is required.");

            // Passive apps must remain visible so their authorized owners/admins can reactivate them.
            // Ignore global filters here, then apply tenant/visibility and lifecycle boundaries explicitly.
            var query = db.Applications.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.RecordState == RecordState.Active || x.RecordState == RecordState.Passive)
                .AsQueryable();

            if (!isSystemAdmin) query = query.Where(x => x.TenantId == tenantId);
            if (!isSystemAdmin && !isTenantAdmin && !isWorkspace)
                query = query.Where(x => x.Status == DefinitionStatus.Published ||
                    (x.CreatedByUserId != null && x.CreatedByUserId == currentUserId));
            return Results.Ok(await query.OrderBy(x => x.Name).Select(x => new 
            { x.Id, 
                x.TenantId, 
                x.Key, 
                x.Name, 
                x.Icon, 
                x.Description, 
                x.ApplicationType, 
                x.Status,
                OperationalState = x.RecordState.ToString(),
                PendingDeletion = db.ApplicationDeletionRequests.AsNoTracking().Where(request => request.ApplicationId == x.Id && request.Status == ApplicationDeletionRequestStatus.Pending)
                    .Select(request => new { request.Id, request.Reason, request.RequestedByUserId, request.CreatedAt }).FirstOrDefault(),
                x.CreatedByUserId,
                CanEdit = !db.ApplicationDeletionRequests.Any(request => request.ApplicationId == x.Id && request.Status == ApplicationDeletionRequestStatus.Pending) &&
                    (isSystemAdmin || isTenantAdmin || isWorkspace || (x.CreatedByUserId != null && x.CreatedByUserId == currentUserId)),
                x.Slugs,
                x.PublishedVersion,
                x.VersionNo,
                x.AppliedTemplateVersion,
                x.TemplateReleaseId,
                x.TemplateId, 
                x.CreatedAt, 
                x.UpdatedAt,
                StartScreenKey = x.MenuItems
                    .Where(menu => menu.Visible && menu.Route == "/builder/applications/" + x.Id + "/screens/" + menu.Key &&
                        x.Screens.Any(screen => screen.Key == menu.Key) &&
                        (isSystemAdmin || isTenantAdmin || isWorkspace || menu.PermissionKey == null))
                    .OrderBy(menu => menu.SortOrder).ThenBy(menu => menu.Key)
                    .Select(menu => menu.Key).FirstOrDefault()
                    ?? x.Screens.OrderBy(screen => screen.Key).Select(screen => screen.Key).FirstOrDefault() }).ToListAsync(token));

        }).RequireAuthorization().WithName("ListBuilderApplications").WithTags("Applications");

        endpoints.MapPost("/applications/{applicationId:guid}/operational-state", async (
            Guid applicationId, SetApplicationOperationalState body, ClaimsPrincipal user,
            BuilderDbContext db, CancellationToken token) =>
        {
            var tenantClaim = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId");
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) &&
                (!Guid.TryParse(tenantClaim, out var claimedTenant) || claimedTenant != body.TenantId))
                return Results.Forbid();

            var app = await db.Applications.IgnoreQueryFilters().SingleOrDefaultAsync(
                x => x.Id == applicationId && x.TenantId == body.TenantId &&
                    (x.RecordState == RecordState.Active || x.RecordState == RecordState.Passive), token);
            if (app is null) return Results.NotFound();
            if (app.Status != DefinitionStatus.Published)
                return Results.Conflict("Only published applications can be activated or put into maintenance mode.");
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();

            app.RecordState = body.IsActive ? RecordState.Active : RecordState.Passive;
            app.UpdatedAt = DateTimeOffset.UtcNow;
            var actorId = ApplicationEditingPolicy.UserId(user);
            app.UpdatedByUserId = actorId;
            db.AuditEvents.Add(new BuilderAuditEvent
            {
                TenantId = app.TenantId,
                EventType = body.IsActive ? "ApplicationActivated" : "ApplicationSetPassive",
                ResourceType = "BuilderApplication",
                ResourceId = app.Id,
                ActorId = actorId,
                DataJson = System.Text.Json.JsonSerializer.Serialize(new { OperationalState = app.RecordState.ToString() })
            });
            await db.SaveChangesAsync(token);
            return Results.Ok(new { app.Id, OperationalState = app.RecordState.ToString(), app.UpdatedAt });
        }).RequireAuthorization().WithName("SetBuilderApplicationOperationalState").WithTags("Applications");

        endpoints.MapPost("/applications", async (CreateApplicationRequest request, ClaimsPrincipal user, BuilderMetadataService service, CancellationToken token) =>
        {
            if (!BuilderApplicationCreationPolicy.CanCreateForTenant(user, request.TenantId, out var tenantId))
                return Results.Forbid();
            try
            {
                return Results.Ok(await service.CreateApplication(request with { TenantId = tenantId }, token, user.IsInRole(BuilderRoleNames.SystemAdmin)));
            }
            catch (ApplicationKeyConflictException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (TemplateAlreadyUsedException ex)
            {
                return Results.Conflict(new { message = ex.Message, templateId = ex.TemplateId,
                    existingApplicationId = ex.ExistingApplicationId, existingApplicationName = ex.ExistingApplicationName });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Applications_TenantId_Key" })
            {
                // Keep the database unique index as the final concurrency guard.
                return Results.Conflict(new { message = ApplicationKeyConflictException.UserMessage });
            }
        })
            .RequireAuthorization()
            .WithName("CreateBuilderApplication")
            .WithTags("Applications");
        return endpoints;
    }
}
