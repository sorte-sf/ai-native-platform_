 
namespace SorteFederation.Builders.Endpoints;

/// <summary>Tenant-admin-only CRUD for users, groups, app roles, permissions and their assignments.</summary>
public static class TenantAccessEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("/tenant-access").WithTags("Tenant access control")
            .RequireAuthorization()
            .AddEndpointFilter(async (context, next) => IsTenantAdmin(context.HttpContext.User)
                ? await next(context) : Results.Forbid());

        admin.MapGet("/builder-workspace/users", async (ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var users = await db.ApplicationUsers.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.RecordState == RecordState.Active)
                .OrderBy(x => x.DisplayName)
                .Select(x => new
                {
                    id = x.Id, x.Email, x.DisplayName, x.IsActive,
                    role = db.WorkspaceAccess.IgnoreQueryFilters().Where(a => a.TenantId == tenantId && a.UserId == x.Id &&
                        a.IsActive && a.RecordState == RecordState.Active).Select(a => a.Role).FirstOrDefault()
                }).ToListAsync(ct);
            return Results.Ok(users);
        });

        admin.MapPut("/builder-workspace/users/{userId:guid}", async (Guid userId, UpdateBuilderWorkspaceAccessRequest request,
            ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var target = await db.ApplicationUsers.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == userId &&
                x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (target is null) return Results.NotFound();
            var role = string.IsNullOrWhiteSpace(request.Role) ? null : request.Role.Trim();
            if (role is not null && role is not ("BuilderAdmin" or "BuilderEditor" or "BuilderViewer"))
                return Results.BadRequest(new { message = "Choose BuilderAdmin, BuilderEditor, BuilderViewer, or no Builder access." });
            if (role is not null && !target.IsActive)
                return Results.Conflict(new { message = "Activate this user before granting Builder workspace access." });

            var grant = await db.WorkspaceAccess.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);
            if (grant is null)
            {
                if (role is null) return Results.NoContent();
                grant = new BuilderWorkspaceAccess { TenantId = tenantId, UserId = userId, Role = role,
                    CreatedByUserId = ApplicationEditingPolicy.UserId(user) };
                db.WorkspaceAccess.Add(grant);
            }
            else if (role is null)
            {
                grant.IsActive = false;
                grant.RecordState = RecordState.Deleted;
                grant.SecurityVersion++;
                grant.UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                if (!grant.IsActive || grant.RecordState != RecordState.Active || grant.Role != role) grant.SecurityVersion++;
                grant.Role = role;
                grant.IsActive = true;
                grant.RecordState = RecordState.Active;
                grant.UpdatedAt = DateTimeOffset.UtcNow;
            }
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { userId, role });
        });

        admin.MapPut("/users/{userId:guid}", async (Guid userId, UpdateAccessUser request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();

            var entity = await db.ApplicationUsers.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);

            if (entity is null) return Results.NotFound();
            var name = (request.DisplayName ?? "").Trim();
            if (name.Length is < 1 or > 200) return Results.BadRequest(new { message = "Display name is required (max 200 characters)." });
            entity.DisplayName = name;
            entity.IsActive = request.IsActive;
            entity.DisabledAt = request.IsActive ? null : entity.DisabledAt ?? DateTimeOffset.UtcNow;
            entity.SecurityVersion++;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { entity.Id, entity.Email, entity.DisplayName, entity.IsActive, entity.MustChangePassword, entity.CreatedAt });
        });

        admin.MapDelete("/users/{userId:guid}", async (Guid userId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var entity = await db.ApplicationUsers.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (entity is null) return Results.NotFound();
            entity.IsActive = false; entity.RecordState = RecordState.Deleted; entity.DisabledAt = DateTimeOffset.UtcNow;
            entity.SecurityVersion++; entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        admin.MapGet("/groups", async (ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            return Results.Ok(await db.UserGroups.IgnoreQueryFilters().AsNoTracking().Where(x => x.TenantId == tenantId && x.RecordState == RecordState.Active)
                .OrderBy(x => x.Name).Select(x => new { x.Id, x.Key, x.Name, x.Description, x.IsActive,
                    memberCount = x.Members.Count(m => m.IsActive && m.RecordState == RecordState.Active) }).ToListAsync(ct));
        });

        admin.MapPost("/groups", async (SaveAccessGroup request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var key = NormalizeKey(request.Key); var name = (request.Name ?? "").Trim();
            if (key.Length is < 1 or > 100 || name.Length is < 1 or > 160) return Results.BadRequest(new { message = "A group key and name are required." });
            if (await db.UserGroups.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Key == key && x.RecordState == RecordState.Active, ct)) return Results.Conflict(new { message = "That group key is already in use." });
            var entity = new BuilderUserGroup { TenantId = tenantId, Key = key, Name = name, Description = CleanDescription(request.Description), IsActive = request.IsActive };
            db.UserGroups.Add(entity); await db.SaveChangesAsync(ct); return Results.Created($"/tenant-access/groups/{entity.Id}", entity);
        });

        admin.MapPut("/groups/{id:guid}", async (Guid id, SaveAccessGroup request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();

            var entity = await db.UserGroups.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (entity is null) return Results.NotFound();

            var key = NormalizeKey(request.Key); var name = (request.Name ?? "").Trim();

            if (key.Length is < 1 or > 100 || name.Length is < 1 or > 160) return Results.BadRequest(new { message = "A group key and name are required." });
           
            if (await db.UserGroups.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Id != id && x.Key == key && x.RecordState == RecordState.Active, ct)) return Results.Conflict(new { message = "That group key is already in use." });

            var changedSecurity = entity.IsActive != request.IsActive || entity.Key != key;

            entity.Key = key; entity.Name = name; entity.Description = CleanDescription(request.Description); entity.IsActive = request.IsActive; entity.UpdatedAt = DateTimeOffset.UtcNow;

            if (changedSecurity) await InvalidateGroupUsers(db, tenantId, [id], ct);
            await db.SaveChangesAsync(ct); return Results.Ok(entity);
        });

        admin.MapDelete("/groups/{id:guid}", async (Guid id, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var entity = await db.UserGroups.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (entity is null) return Results.NotFound(); await InvalidateGroupUsers(db, tenantId, [id], ct); entity.RecordState = RecordState.Deleted; entity.IsActive = false; entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct); return Results.NoContent();
        });

        admin.MapGet("/groups/{id:guid}/members", async (Guid id, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            return Results.Ok(await db.UserGroupMemberships.IgnoreQueryFilters().AsNoTracking().Where(x => x.TenantId == tenantId && x.GroupId == id && x.RecordState == RecordState.Active)
                .Select(x => new { x.Id, x.UserId, x.User.Email, x.User.DisplayName, x.IsActive }).ToListAsync(ct));
        });

        admin.MapPut("/groups/{groupId:guid}/members/{userId:guid}", async (Guid groupId, Guid userId, SetActiveAssignment request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();

            if (!await db.UserGroups.IgnoreQueryFilters().AnyAsync(x => x.Id == groupId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct) ||
                !await db.ApplicationUsers.IgnoreQueryFilters().AnyAsync(x => x.Id == userId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct)) return Results.NotFound();

            var link = await db.UserGroupMemberships.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.GroupId == groupId && x.UserId == userId, ct);

            if (link is null) { link = new BuilderUserGroupMembership { TenantId = tenantId, GroupId = groupId, UserId = userId, IsActive = request.IsActive }; db.UserGroupMemberships.Add(link); }

            else { link.IsActive = request.IsActive; link.RecordState = RecordState.Active; link.UpdatedAt = DateTimeOffset.UtcNow; }

            await InvalidateUsers(db, tenantId, [userId], ct);
            await db.SaveChangesAsync(ct); return Results.Ok(new { link.Id, link.GroupId, link.UserId, link.IsActive });
        });

        admin.MapDelete("/groups/{groupId:guid}/members/{userId:guid}", async (Guid groupId, Guid userId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var link = await db.UserGroupMemberships.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.GroupId == groupId && x.UserId == userId && x.RecordState == RecordState.Active, ct);
            if (link is null) return Results.NotFound(); await InvalidateUsers(db, tenantId, [userId], ct); link.RecordState = RecordState.Deleted; link.IsActive = false; link.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct); return Results.NoContent();
        });

        admin.MapGet("/roles", async (ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            return Results.Ok(await db.ApplicationRoles.IgnoreQueryFilters().AsNoTracking().Where(x => x.TenantId == tenantId && x.RecordState == RecordState.Active).OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Key, x.Name, x.Description, x.IsActive, x.IsSystemRole }).ToListAsync(ct));
        });

        admin.MapPost("/roles", async (SaveAccessRole request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var key = NormalizeKey(request.Key); var name = (request.Name ?? "").Trim();
            if (key.Length is < 1 or > 100 || name.Length is < 1 or > 160) return Results.BadRequest(new { message = "A role key and name are required." });
            if (await db.ApplicationRoles.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Key == key && x.RecordState == RecordState.Active, ct)) return Results.Conflict(new { message = "That role key is already in use." });
            var entity = new BuilderApplicationRole { TenantId = tenantId, Key = key, Name = name, Description = CleanDescription(request.Description), IsActive = request.IsActive };
            db.ApplicationRoles.Add(entity); await db.SaveChangesAsync(ct); return Results.Created($"/tenant-access/roles/{entity.Id}", entity);
        });

        admin.MapPut("/roles/{id:guid}", async (Guid id, SaveAccessRole request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var entity = await db.ApplicationRoles.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (entity is null) return Results.NotFound();
            if (entity.IsSystemRole) return Results.Conflict(new { message = "System roles cannot be edited." });
            var key = NormalizeKey(request.Key); var name = (request.Name ?? "").Trim();
            if (key.Length is < 1 or > 100 || name.Length is < 1 or > 160) return Results.BadRequest(new { message = "A role key and name are required." });
            if (await db.ApplicationRoles.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Id != id && x.Key == key && x.RecordState == RecordState.Active, ct)) return Results.Conflict(new { message = "That role key is already in use." });
            var affectedGroups = await db.GroupRoleAssignments.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && x.RoleId == id && x.RecordState == RecordState.Active).Select(x => x.GroupId).Distinct().ToArrayAsync(ct);
            var changedSecurity = entity.Key != key || entity.IsActive != request.IsActive;
            entity.Key = key; entity.Name = name; entity.Description = CleanDescription(request.Description); entity.IsActive = request.IsActive; entity.UpdatedAt = DateTimeOffset.UtcNow;
            if (changedSecurity) await InvalidateGroupUsers(db, tenantId, affectedGroups, ct);
            await db.SaveChangesAsync(ct); return Results.Ok(entity);
        });

        admin.MapDelete("/roles/{id:guid}", async (Guid id, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var entity = await db.ApplicationRoles.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (entity is null) return Results.NotFound(); if (entity.IsSystemRole) return Results.Conflict(new { message = "System roles cannot be deleted." });
            var affectedGroups = await db.GroupRoleAssignments.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && x.RoleId == id && x.RecordState == RecordState.Active).Select(x => x.GroupId).Distinct().ToArrayAsync(ct);
            await InvalidateGroupUsers(db, tenantId, affectedGroups, ct);
            entity.RecordState = RecordState.Deleted; entity.IsActive = false; entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct); return Results.NoContent();
        });

        admin.MapGet("/applications/{applicationId:guid}/group-roles", async (Guid applicationId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            return Results.Ok(await db.GroupRoleAssignments.IgnoreQueryFilters().AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.RecordState == RecordState.Active)
                .Select(x => new { x.Id, x.GroupId, groupName = x.Group.Name, x.RoleId, roleName = x.Role.Name, roleKey = x.Role.Key, x.IsActive }).ToListAsync(ct));
        });
        admin.MapPut("/applications/{applicationId:guid}/groups/{groupId:guid}/roles/{roleId:guid}", async (Guid applicationId, Guid groupId, Guid roleId, SetActiveAssignment request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            if (!await db.Applications.IgnoreQueryFilters().AnyAsync(x => x.Id == applicationId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct) ||
                !await db.UserGroups.IgnoreQueryFilters().AnyAsync(x => x.Id == groupId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct) ||
                !await db.ApplicationRoles.IgnoreQueryFilters().AnyAsync(x => x.Id == roleId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct)) return Results.NotFound();
            var link = await db.GroupRoleAssignments.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.GroupId == groupId && x.RoleId == roleId, ct);
            if (link is null) { link = new BuilderGroupRoleAssignment { TenantId = tenantId, ApplicationId = applicationId, GroupId = groupId, RoleId = roleId, IsActive = request.IsActive }; db.GroupRoleAssignments.Add(link); }
            else { link.IsActive = request.IsActive; link.RecordState = RecordState.Active; link.UpdatedAt = DateTimeOffset.UtcNow; }
            await InvalidateGroupUsers(db, tenantId, [groupId], ct);
            await db.SaveChangesAsync(ct); return Results.Ok(new { link.Id, link.ApplicationId, link.GroupId, link.RoleId, link.IsActive });
        });

        admin.MapDelete("/applications/{applicationId:guid}/groups/{groupId:guid}/roles/{roleId:guid}", async (Guid applicationId, Guid groupId, Guid roleId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var link = await db.GroupRoleAssignments.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.GroupId == groupId && x.RoleId == roleId && x.RecordState == RecordState.Active, ct);
            if (link is null) return Results.NotFound(); await InvalidateGroupUsers(db, tenantId, [groupId], ct); link.IsActive = false; link.RecordState = RecordState.Deleted; link.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct); return Results.NoContent();
        });

        admin.MapGet("/applications/{applicationId:guid}/permissions", async (Guid applicationId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            return Results.Ok(await db.Permissions.IgnoreQueryFilters().AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.RecordState == RecordState.Active)
                .OrderBy(x => x.Name).Select(x => new { x.Id, x.ApplicationId, x.Key, x.Name, x.ResourceType, x.Action, x.ObjectId, x.RecordId }).ToListAsync(ct));
        });

        admin.MapPost("/applications/{applicationId:guid}/permissions", async (Guid applicationId, SaveAccessPermission request, ClaimsPrincipal user, BuilderDbContext db, TenantRuntimeContextFactory runtimeFactory, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var key = NormalizeKey(request.Key); var name = (request.Name ?? "").Trim(); var resource = (request.ResourceType ?? "object").Trim().ToLowerInvariant(); var action = (request.Action ?? "").Trim().ToLowerInvariant();
            if (key.Length is < 1 or > 120 || name.Length is < 1 or > 200 || resource is not ("object" or "application") || action is not ("read" or "create" or "update" or "delete") || resource == "object" && !request.ObjectId.HasValue || resource == "application" && request.ObjectId.HasValue) return Results.BadRequest(new { message = "Provide a key, name, valid resource scope, and action. Object-scoped permissions require a table." });
            if (!await db.Applications.IgnoreQueryFilters().AnyAsync(x => x.Id == applicationId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct)) return Results.NotFound();
            if (request.RecordId.HasValue && !request.ObjectId.HasValue) return Results.BadRequest(new { message = "Record scope requires an object/table." });
            if (request.ObjectId.HasValue && !await db.Objects.IgnoreQueryFilters().AnyAsync(x => x.Id == request.ObjectId && x.ApplicationId == applicationId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct)) return Results.BadRequest(new { message = "The selected table does not belong to this application." });
            if (request.RecordId.HasValue)
            {
                await using var runtime = await runtimeFactory.CreateAsync(tenantId, ct);
                if (!await runtime.Records.AnyAsync(x => x.Id == request.RecordId && x.ObjectId == request.ObjectId && x.TenantId == tenantId, ct)) return Results.BadRequest(new { message = "The selected record does not belong to this table." });
            }
            if (await db.Permissions.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.Key == key && x.RecordState == RecordState.Active, ct)) return Results.Conflict(new { message = "That permission key already exists for this application." });
            var entity = new BuilderPermission { TenantId = tenantId, ApplicationId = applicationId, Key = key, Name = name, ResourceType = resource, Action = action, ObjectId = request.ObjectId, RecordId = request.RecordId };
            db.Permissions.Add(entity); await db.SaveChangesAsync(ct); return Results.Created($"/tenant-access/permissions/{entity.Id}", entity);
        });

        admin.MapPut("/permissions/{id:guid}", async (Guid id, SaveAccessPermission request, ClaimsPrincipal user, BuilderDbContext db, TenantRuntimeContextFactory runtimeFactory, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var entity = await db.Permissions.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (entity is null) return Results.NotFound();
            var key = NormalizeKey(request.Key); var name = (request.Name ?? "").Trim(); var resource = (request.ResourceType ?? "object").Trim().ToLowerInvariant(); var action = (request.Action ?? "").Trim().ToLowerInvariant();
            if (key.Length is < 1 or > 120 || name.Length is < 1 or > 200 || resource is not ("object" or "application") || action is not ("read" or "create" or "update" or "delete") || resource == "object" && !request.ObjectId.HasValue || resource == "application" && request.ObjectId.HasValue) return Results.BadRequest(new { message = "Permission details or scope are invalid." });
            if (request.RecordId.HasValue && !request.ObjectId.HasValue) return Results.BadRequest(new { message = "Record scope requires an object/table." });
            if (request.ObjectId.HasValue && !await db.Objects.IgnoreQueryFilters().AnyAsync(x => x.Id == request.ObjectId && x.ApplicationId == entity.ApplicationId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct)) return Results.BadRequest(new { message = "The selected table does not belong to this application." });
            if (request.RecordId.HasValue)
            {
                await using var runtime = await runtimeFactory.CreateAsync(tenantId, ct);
                if (!await runtime.Records.AnyAsync(x => x.Id == request.RecordId && x.ObjectId == request.ObjectId && x.TenantId == tenantId, ct)) return Results.BadRequest(new { message = "The selected record does not belong to this table." });
            }
            entity.Key = key; entity.Name = name; entity.ResourceType = resource; entity.Action = action; entity.ObjectId = request.ObjectId; entity.RecordId = request.RecordId; entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct); return Results.Ok(entity);
        });

        admin.MapDelete("/permissions/{id:guid}", async (Guid id, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var entity = await db.Permissions.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (entity is null) return Results.NotFound(); entity.RecordState = RecordState.Deleted; entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct); return Results.NoContent();
        });

        admin.MapGet("/applications/{applicationId:guid}/permission-grants", async (Guid applicationId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            return Results.Ok(await db.PermissionGrants.IgnoreQueryFilters().AsNoTracking().Where(x => x.TenantId == tenantId && x.RecordState == RecordState.Active && x.Permission.ApplicationId == applicationId)
                .Select(x => new { x.Id, x.PermissionId, permissionName = x.Permission.Name, x.SubjectType, x.SubjectId }).ToListAsync(ct));
        });

        admin.MapPost("/applications/{applicationId:guid}/permission-grants", async (Guid applicationId, SaveAccessGrant request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var permission = await db.Permissions.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == request.PermissionId && x.ApplicationId == applicationId && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (permission is null) return Results.NotFound();
            var type = (request.SubjectType ?? "").Trim().ToLowerInvariant(); var subject = request.SubjectId?.Trim() ?? "";
            var exists = type switch
            {
                "user" when Guid.TryParse(subject, out var uid) => await db.ApplicationUsers.IgnoreQueryFilters().AnyAsync(x => x.Id == uid && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct),
                "group" when Guid.TryParse(subject, out var gid) => await db.UserGroups.IgnoreQueryFilters().AnyAsync(x => x.Id == gid && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct),
                "role" => await db.ApplicationRoles.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Key == subject && x.IsActive && x.RecordState == RecordState.Active, ct),
                _ => false
            };
            if (!exists) return Results.BadRequest(new { message = "Grant subject must be a user/group ID or an active role key in this tenant." });
            var grant = await db.PermissionGrants.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.PermissionId == permission.Id && x.SubjectType == type && x.SubjectId == subject, ct);
            if (grant is null) { grant = new BuilderPermissionGrant { TenantId = tenantId, PermissionId = permission.Id, SubjectType = type, SubjectId = subject }; db.PermissionGrants.Add(grant); }
            else { grant.RecordState = RecordState.Active; grant.UpdatedAt = DateTimeOffset.UtcNow; }
            await db.SaveChangesAsync(ct); return Results.Ok(new { grant.Id, grant.PermissionId, grant.SubjectType, grant.SubjectId });
        });

        admin.MapDelete("/permission-grants/{id:guid}", async (Guid id, ClaimsPrincipal user, BuilderDbContext db, CancellationToken ct) =>
        {
            if (!TryTenant(user, out var tenantId)) return Results.Forbid();
            var grant = await db.PermissionGrants.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.RecordState == RecordState.Active, ct);
            if (grant is null) return Results.NotFound(); grant.RecordState = RecordState.Deleted; grant.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct); return Results.NoContent();
        });
    }

    private static bool IsTenantAdmin(ClaimsPrincipal user) => user.IsInRole(BuilderRoleNames.SystemAdmin) || user.IsInRole(BuilderRoleNames.TenantAdmin);
    private static bool TryTenant(ClaimsPrincipal user, out Guid tenantId) => Guid.TryParse(user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId"), out tenantId) && tenantId != Guid.Empty;
    private static string NormalizeKey(string? value) => (value ?? "").Trim().ToLowerInvariant().Replace(' ', '-');
    private static string? CleanDescription(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, 1000)];

    private static async Task InvalidateGroupUsers(BuilderDbContext db, Guid tenantId, Guid[] groupIds, CancellationToken ct)
    {
        if (groupIds.Length == 0) return;
        var userIds = await db.UserGroupMemberships.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && groupIds.Contains(x.GroupId) && x.RecordState == RecordState.Active)
            .Select(x => x.UserId).Distinct().ToArrayAsync(ct);
        await InvalidateUsers(db, tenantId, userIds, ct);
    }

    private static async Task InvalidateUsers(BuilderDbContext db, Guid tenantId, Guid[] userIds, CancellationToken ct)
    {
        if (userIds.Length == 0) return;
        var users = await db.ApplicationUsers.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && userIds.Contains(x.Id) && x.RecordState == RecordState.Active).ToListAsync(ct);
        foreach (var user in users) user.SecurityVersion++;
    }
}

public sealed record UpdateAccessUser(string DisplayName, bool IsActive);
public sealed record SaveAccessGroup(string Key, string Name, string? Description, bool IsActive = true);
public sealed record SaveAccessRole(string Key, string Name, string? Description, bool IsActive = true);
public sealed record SetActiveAssignment(bool IsActive = true);
public sealed record SaveAccessPermission(string Key, string Name, string ResourceType, string Action, Guid? ObjectId, Guid? RecordId);
public sealed record SaveAccessGrant(Guid PermissionId, string SubjectType, string SubjectId);
