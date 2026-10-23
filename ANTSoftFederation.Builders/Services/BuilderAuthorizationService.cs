using System.Security.Claims;
using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Services;

public sealed class BuilderAuthorizationService(BuilderDbContext db)
{
    public async Task<object> ObjectCapabilities(Guid tenantId, Guid objectId, ClaimsPrincipal user, CancellationToken token)
    {
        var applicationId = await ResolveApplicationId(tenantId, objectId, user, token);
        if (IsTenantAdministrator(user, tenantId)) return new { Read = true, Create = true, Update = true, Delete = true };
        var permissions = await db.Permissions.Where(p => p.TenantId == tenantId && p.ApplicationId == applicationId &&
            ((p.ResourceType == "object" && (p.ObjectId == null || p.ObjectId == objectId)) || (p.ResourceType == "application" && p.ObjectId == null)) && p.RecordId == null)
            .ToListAsync(token);
        var granted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var permission in permissions)
            if (await HasGrant(tenantId, [permission.Id], user, token)) granted.Add(permission.Action);
        return new { Read = granted.Contains("read"), Create = granted.Contains("create"), Update = granted.Contains("update"), Delete = granted.Contains("delete") };
    }

    public async Task EnsureRecordAccess(Guid tenantId, Guid objectId, Guid recordId, string action, ClaimsPrincipal user, CancellationToken token)
    {
        if (IsTenantAdministrator(user, tenantId)) return;
        var applicationId = await ResolveApplicationId(tenantId, objectId, user, token);
        var policyPermissions = await db.Permissions.Where(p => p.TenantId == tenantId && p.ApplicationId == applicationId &&
            ((p.ResourceType == "object" && (p.ObjectId == null || p.ObjectId == objectId)) || (p.ResourceType == "application" && p.ObjectId == null)) && p.Action == action)
            .Select(p => new { p.Id, p.RecordId }).ToListAsync(token);
        if (policyPermissions.Count == 0) throw Denied(action);
        var tableIds = policyPermissions.Where(p => p.RecordId == null).Select(p => p.Id).ToArray();
        var specificIds = policyPermissions.Where(p => p.RecordId == recordId).Select(p => p.Id).ToArray();
        if (tableIds.Length > 0 && await HasGrant(tenantId, tableIds, user, token)) return;
        if (specificIds.Length > 0 && await HasGrant(tenantId, specificIds, user, token)) return;
        throw Denied(action);
    }

    public async Task EnsureObjectAccess(Guid tenantId, Guid objectId, string action, ClaimsPrincipal user, CancellationToken token)
    {
        if (IsTenantAdministrator(user, tenantId)) return;
        var applicationId = await ResolveApplicationId(tenantId, objectId, user, token);
        var permissionIds = await db.Permissions.Where(p => p.TenantId == tenantId && p.ApplicationId == applicationId &&
            ((p.ResourceType == "object" && (p.ObjectId == null || p.ObjectId == objectId)) || (p.ResourceType == "application" && p.ObjectId == null)) && p.Action == action && p.RecordId == null)
            .Select(p => p.Id).ToListAsync(token);
        if (permissionIds.Count > 0 && await HasGrant(tenantId, permissionIds, user, token)) return;
        // A user with record-scoped grants may query the table, but the query path must filter to those records.
        var recordGrants = await (from permission in db.Permissions
            join grant in db.PermissionGrants on permission.Id equals grant.PermissionId
            where permission.TenantId == tenantId && permission.ApplicationId == applicationId && permission.ResourceType == "object" &&
                permission.Action == action && permission.ObjectId == objectId && permission.RecordId != null &&
                grant.TenantId == tenantId
            select new { grant.SubjectType, grant.SubjectId }).ToListAsync(token);
        if (!recordGrants.Any(grant => SubjectKeys(user).Contains((grant.SubjectType, grant.SubjectId)))) throw Denied(action);
    }

    public async Task<List<BuilderRecord>> FilterRecords(Guid tenantId, Guid objectId, string action,
        IReadOnlyCollection<BuilderRecord> records, ClaimsPrincipal user, CancellationToken token)
    {
        if (records.Count == 0 || IsTenantAdministrator(user, tenantId)) return records.ToList();
        var applicationId = await ResolveApplicationId(tenantId, objectId, user, token);
        var tablePermissionIds = await db.Permissions.Where(p => p.TenantId == tenantId && p.ApplicationId == applicationId &&
            ((p.ResourceType == "object" && (p.ObjectId == null || p.ObjectId == objectId)) || (p.ResourceType == "application" && p.ObjectId == null)) && p.Action == action && p.RecordId == null)
            .Select(p => p.Id).ToListAsync(token);
        if (tablePermissionIds.Count > 0 && await HasGrant(tenantId, tablePermissionIds, user, token)) return records.ToList();
        var recordIds = records.Select(x => x.Id).ToArray();
        var allowed = await (from permission in db.Permissions
            join grant in db.PermissionGrants on permission.Id equals grant.PermissionId
            where permission.TenantId == tenantId && permission.ApplicationId == applicationId && permission.ResourceType == "object" &&
                permission.Action == action && permission.ObjectId == objectId && permission.RecordId != null &&
                recordIds.Contains(permission.RecordId.Value) && grant.TenantId == tenantId
            select new { permission.RecordId, grant.SubjectType, grant.SubjectId })
            .ToListAsync(token);
        var subjects = SubjectKeys(user);
        var allowedIds = allowed.Where(x => subjects.Contains((x.SubjectType, x.SubjectId)))
            .Select(x => x.RecordId!.Value).ToHashSet();
        return records.Where(x => allowedIds.Contains(x.Id)).ToList();
    }

    /// <returns>Null when the caller has table-wide access; otherwise the exact permitted record IDs.</returns>
    public async Task<Guid[]?> RecordGrantIds(Guid tenantId, Guid objectId, string action, ClaimsPrincipal user, CancellationToken token)
    {
        if (IsTenantAdministrator(user, tenantId)) return null;
        var applicationId = await ResolveApplicationId(tenantId, objectId, user, token);
        var tablePermissionIds = await db.Permissions.Where(p => p.TenantId == tenantId && p.ApplicationId == applicationId &&
            ((p.ResourceType == "object" && (p.ObjectId == null || p.ObjectId == objectId)) || (p.ResourceType == "application" && p.ObjectId == null)) && p.Action == action && p.RecordId == null)
            .Select(p => p.Id).ToListAsync(token);
        if (tablePermissionIds.Count > 0 && await HasGrant(tenantId, tablePermissionIds, user, token)) return null;

        var subjects = SubjectKeys(user);
        var ids = await (from permission in db.Permissions
            join grant in db.PermissionGrants on permission.Id equals grant.PermissionId
            where permission.TenantId == tenantId && permission.ApplicationId == applicationId && permission.ResourceType == "object" &&
                permission.Action == action && permission.ObjectId == objectId && permission.RecordId != null && grant.TenantId == tenantId
            select new { permission.RecordId, grant.SubjectType, grant.SubjectId }).ToListAsync(token);
        return ids.Where(x => subjects.Contains((x.SubjectType, x.SubjectId))).Select(x => x.RecordId!.Value).Distinct().ToArray();
    }

    private async Task<Guid> ResolveApplicationId(Guid tenantId, Guid objectId, ClaimsPrincipal user, CancellationToken token)
    {
        var applicationId = await db.Objects.Where(x => x.Id == objectId && x.TenantId == tenantId)
            .Select(x => (Guid?)x.ApplicationId).SingleOrDefaultAsync(token)
            ?? throw new KeyNotFoundException("Object was not found for this tenant.");
        if (IsBuilderApplicationUser(user) &&
            (!Guid.TryParse(user.FindFirstValue("builder_application_id"), out var tokenApplicationId) || tokenApplicationId != applicationId))
            throw new UnauthorizedAccessException("This application-user token is not valid for the requested application.");
        return applicationId;
    }

    private async Task<bool> HasGrant(Guid tenantId, IReadOnlyCollection<Guid> permissionIds, ClaimsPrincipal user, CancellationToken token)
    {
        if (permissionIds.Count == 0) return false;
        var userId = UserId(user);
        var groups = user.FindAll("builder_group_id").Select(x => x.Value).ToArray();
        var roles = user.FindAll("builder_app_role").Concat(user.FindAll(ClaimTypes.Role)).Concat(user.FindAll("role"))
            .Select(x => x.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return await db.PermissionGrants.AnyAsync(x => x.TenantId == tenantId && permissionIds.Contains(x.PermissionId) &&
            (x.SubjectType == "user" && userId != null && x.SubjectId == userId ||
             x.SubjectType == "group" && groups.Contains(x.SubjectId) ||
             x.SubjectType == "role" && roles.Contains(x.SubjectId)), token);
    }

    private static HashSet<(string Type, string Id)> SubjectKeys(ClaimsPrincipal user)
    {
        var keys = new HashSet<(string, string)>(StringTupleComparer.OrdinalIgnoreCase);
        var userId = UserId(user);
        if (userId is not null) keys.Add(("user", userId));
        foreach (var group in user.FindAll("builder_group_id")) keys.Add(("group", group.Value));
        foreach (var role in user.FindAll("builder_app_role").Concat(user.FindAll(ClaimTypes.Role)).Concat(user.FindAll("role")))
            keys.Add(("role", role.Value));
        return keys;
    }

    private static string? UserId(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? user.FindFirstValue("user_id");
    private static bool IsBuilderApplicationUser(ClaimsPrincipal user) => user.HasClaim("builder_user", "true");
    private static bool IsTenantAdministrator(ClaimsPrincipal user, Guid tenantId) =>
        user.IsInRole(BuilderRoleNames.SystemAdmin) ||
        user.IsInRole(BuilderRoleNames.TenantAdmin) &&
        Guid.TryParse(user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId"), out var claimTenant) && claimTenant == tenantId;
    private static UnauthorizedAccessException Denied(string action) => new($"The current user is not authorized to {action} this resource.");

    private sealed class StringTupleComparer : IEqualityComparer<(string Type, string Id)>
    {
        public static readonly StringTupleComparer OrdinalIgnoreCase = new();
        public bool Equals((string Type, string Id) x, (string Type, string Id) y) =>
            string.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Id, y.Id, StringComparison.OrdinalIgnoreCase);
        public int GetHashCode((string Type, string Id) value) => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(value.Type), StringComparer.OrdinalIgnoreCase.GetHashCode(value.Id));
    }
}
