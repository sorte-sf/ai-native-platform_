using System.Text.RegularExpressions;
using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data;
using SorteFederation.Builders.Data.Domain;
using SorteFederation.Builders.Services;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.Builders.Endpoints;

public sealed class ApplicationMenuEndpoints : IAppEndPoints
{
    private static readonly Regex MenuKeyPattern = new("^[a-z][a-z0-9-]{0,79}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapApplicationMenuEndpoints(endpoints);

    public static IEndpointRouteBuilder MapApplicationMenuEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/application-menu").RequireAuthorization().WithTags("Application menu");

        group.MapGet("", async (Guid tenantId, Guid applicationId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            var app = await db.Applications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status == DefinitionStatus.Published)
            {
                var version = await db.ApplicationVersions.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.ApplicationId == applicationId && x.TenantId == tenantId && x.VersionNumber == app.PublishedVersion, token);
                if (version is null) return Results.NotFound();
                using var document = JsonDocument.Parse(version.SnapshotJson);
                if (!document.RootElement.TryGetProperty("Menus", out var menus)) return Results.Conflict("Republish this legacy application to generate a complete runtime definition.");
                return Results.Ok(menus.EnumerateArray().Select(item => new {
                    Id = item.GetProperty("Id").GetGuid(), TenantId = tenantId, ApplicationId = applicationId,
                    ParentId = item.GetProperty("ParentId").ValueKind == JsonValueKind.Null ? (Guid?)null : item.GetProperty("ParentId").GetGuid(),
                    ModuleId = item.GetProperty("ModuleId").ValueKind == JsonValueKind.Null ? (Guid?)null : item.GetProperty("ModuleId").GetGuid(),
                    Key = item.GetProperty("Key").GetString(), Label = item.GetProperty("Label").GetString(),
                    Route = item.GetProperty("Route").GetString(), Icon = item.GetProperty("Icon").GetString(),
                    SortOrder = item.GetProperty("SortOrder").GetInt32(), Visible = item.GetProperty("Visible").GetBoolean(),
                    PermissionKey = item.GetProperty("PermissionKey").GetString()
                }).OrderBy(x => x.SortOrder).ToArray());
            }
            return Results.Ok(await db.ApplicationMenuItems.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId).OrderBy(x => x.SortOrder).ToListAsync(token));
        });

        group.MapPost("", async (CreateApplicationMenuItemRequest request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token);
            if (app is null) return Results.NotFound("Application was not found.");
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Only draft applications can be edited.");
            var invalid = Validate(request.Key, request.Label, request.Route);
            if (invalid is not null) return Results.BadRequest(invalid);
            if (request.ParentId is not null && !await db.ApplicationMenuItems.AnyAsync(x => x.Id == request.ParentId && x.ApplicationId == request.ApplicationId && x.TenantId == request.TenantId, token)) return Results.BadRequest("Parent menu item was not found.");
            if (request.ModuleId is Guid moduleId && !await db.ApplicationModules.AnyAsync(x => x.Id == moduleId && x.ApplicationId == request.ApplicationId && x.TenantId == request.TenantId, token)) return Results.BadRequest("Module was not found for this application.");
            if (!await ValidPageRoute(db, request.ApplicationId, request.TenantId, request.Route, token)) return Results.BadRequest("Menu links must target a page in this application.");
            if (await db.ApplicationMenuItems.AnyAsync(x => x.ApplicationId == request.ApplicationId && x.TenantId == request.TenantId && x.Key == request.Key, token)) return Results.Conflict("A menu item with this key already exists.");
            var item = new BuilderApplicationMenuItem {
                TenantId = request.TenantId, ApplicationId = request.ApplicationId, ParentId = request.ParentId,
                Key = request.Key.Trim(), Label = request.Label.Trim(), Route = Normalize(request.Route), Icon = request.Icon?.Trim(),
                SortOrder = request.SortOrder, Visible = request.Visible, PermissionKey = request.PermissionKey?.Trim(), ModuleId = request.ModuleId
            };
            db.ApplicationMenuItems.Add(item);
            await db.SaveChangesAsync(token);
            return Results.Created($"/application-menu/{item.Id}", item);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateApplicationMenuItemRequest request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            var item = await db.ApplicationMenuItems.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == request.TenantId, token);
            if (item is null) return Results.NotFound();
            var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == item.ApplicationId && x.TenantId == request.TenantId, token);
            if (app is null) return Results.NotFound("Application was not found.");
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Only draft applications can be edited.");
            var invalid = Validate(request.Key, request.Label, request.Route);
            if (invalid is not null) return Results.BadRequest(invalid);
            if (request.ParentId is Guid parentId && !await db.ApplicationMenuItems.AnyAsync(x => x.Id == parentId && x.ApplicationId == item.ApplicationId && x.TenantId == request.TenantId, token)) return Results.BadRequest("Invalid parent menu item.");
            if (request.ParentId == id || await CreatesCycle(db, item, request.ParentId, token)) return Results.BadRequest("A menu item cannot be nested inside itself or one of its descendants.");
            if (request.ModuleId is Guid moduleId && !await db.ApplicationModules.AnyAsync(x => x.Id == moduleId && x.ApplicationId == item.ApplicationId && x.TenantId == request.TenantId, token)) return Results.BadRequest("Module was not found for this application.");
            if (!await ValidPageRoute(db, item.ApplicationId, request.TenantId, request.Route, token)) return Results.BadRequest("Menu links must target a page in this application.");
            if (await db.ApplicationMenuItems.AnyAsync(x => x.Id != id && x.ApplicationId == item.ApplicationId && x.TenantId == request.TenantId && x.Key == request.Key, token)) return Results.Conflict("A menu item with this key already exists.");
            item.ModuleId = request.ModuleId; item.ParentId = request.ParentId; item.Key = request.Key.Trim();
            item.Label = request.Label.Trim(); item.Route = Normalize(request.Route); item.Icon = request.Icon?.Trim();
            item.SortOrder = request.SortOrder; item.Visible = request.Visible; item.PermissionKey = request.PermissionKey?.Trim();
            item.Version++; item.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return Results.Ok(item);
        });

        group.MapDelete("/{id:guid}", async (Guid id, Guid tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            var item = await db.ApplicationMenuItems.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, token);
            if (item is null) return Results.NotFound();
            var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == item.ApplicationId && x.TenantId == tenantId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Only draft applications can be edited.");
            db.ApplicationMenuItems.Remove(item);
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        });

        return endpoints;
    }

    private static string? Validate(string? key, string? label, string? route)
    {
        if (string.IsNullOrWhiteSpace(key) || !MenuKeyPattern.IsMatch(key.Trim())) return "Menu keys must start with a lowercase letter and contain only lowercase letters, numbers, or hyphens (up to 80 characters).";

        if (string.IsNullOrWhiteSpace(label) || label.Trim().Length > 80) return "Menu labels must contain 1–80 characters.";

        if (!string.IsNullOrWhiteSpace(route) && (route.Length > 512 || !route.StartsWith("/builder/applications/", StringComparison.Ordinal) || route.Contains("..", StringComparison.Ordinal) || route.Contains('\\'))) return "Menu links must use an internal application page route.";

        return null;
    }

    private static string? Normalize(string? route) => string.IsNullOrWhiteSpace(route) ? null : route.Trim();

    private static async Task<bool> ValidPageRoute(BuilderDbContext db, Guid applicationId, Guid tenantId, string? route, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(route)) return true;
        var routePrefix = $"/builder/applications/{applicationId}/screens/";
        if (!route.StartsWith(routePrefix, StringComparison.Ordinal)) return false;
        var pageKey = route[routePrefix.Length..];
        if (string.IsNullOrWhiteSpace(pageKey) || pageKey.Contains('/') || pageKey.Contains('?') || pageKey.Contains('#')) return false;
        return await db.ApplicationScreens.AnyAsync(x => x.ApplicationId == applicationId && x.TenantId == tenantId && x.Key == pageKey, token);
    }

    private static async Task<bool> CreatesCycle(BuilderDbContext db, BuilderApplicationMenuItem item, Guid? proposedParentId, CancellationToken token)
    {
        var visited = new HashSet<Guid>();
        while (proposedParentId is Guid parentId && visited.Add(parentId))
        {
            if (parentId == item.Id) return true;
            proposedParentId = await db.ApplicationMenuItems.Where(x => x.Id == parentId && x.ApplicationId == item.ApplicationId && x.TenantId == item.TenantId)
                .Select(x => x.ParentId).SingleOrDefaultAsync(token);
        }
        return false;
    }
}
