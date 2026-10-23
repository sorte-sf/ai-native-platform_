namespace SorteFederation.Builders.Endpoints;

public sealed class ApplicationScreensEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPatch("/applications/{applicationId:guid}/screens/{screenId:guid}/title", async (
            Guid applicationId, Guid screenId, RenameApplicationScreenRequest request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) => {
            if (!CanDesign(user) || !CanAccessTenant(user, request.TenantId) || request.TenantId == Guid.Empty) return Results.Forbid();
            if (!TryNormalizePageTitle(request.Title, out var title))
                return Results.BadRequest("Page name must be 1–120 characters and use only letters, numbers, spaces, and basic punctuation.");
            var app = await db.Applications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Only draft applications can be edited.");
            var screen = await db.ApplicationScreens.SingleOrDefaultAsync(x => x.Id == screenId && x.ApplicationId == applicationId && x.TenantId == request.TenantId, token);
            if (screen is null) return Results.NotFound();
            var existingTitles = await db.ApplicationScreens
                .Where(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId && x.Id != screenId)
                .Select(x => x.Title).ToListAsync(token);
            if (existingTitles.Any(existing => string.Equals(NormalizePageTitle(existing), title, StringComparison.OrdinalIgnoreCase)))
                return Results.Conflict(new { code = "page_name_exists", message = "A page with this name already exists in this application." });
            var previousTitle = screen.Title;
            screen.Title = title; screen.UpdatedAt = DateTimeOffset.UtcNow;
            var route = $"/builder/applications/{applicationId}/screens/{screen.Key}";
            var menus = await db.ApplicationMenuItems.Where(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId && x.Route == route && x.Label == previousTitle).ToListAsync(token);
            foreach (var menu in menus) { menu.Label = title; menu.UpdatedAt = DateTimeOffset.UtcNow; }
            await db.SaveChangesAsync(token);
            return Results.Ok(new { screen.Id, screen.Title });
        }).RequireAuthorization().WithTags("Application Screens");

        endpoints.MapGet("/applications/{applicationId:guid}/screens", async (
            Guid applicationId, Guid tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!CanDesign(user) || !CanAccessTenant(user, tenantId)) return Results.Forbid();
            var app = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);
            if (app is null || app.RecordState != RecordState.Active) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Draft screens are only available in the designer before publishing.");
            var screens = await db.ApplicationScreens.AsNoTracking()
                .Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new { x.Id, x.Key, x.Title, x.HtmlContent, x.ObjectId, x.FormKey, x.FormId, x.DefinitionJson })
                .ToListAsync(token);
            return Results.Ok(screens);
        }).RequireAuthorization().WithTags("Application Screens").WithName("ListBuilderApplicationScreens");

        endpoints.MapDelete("/applications/{applicationId:guid}/screens/{screenId:guid}", async (
            Guid applicationId, Guid screenId, Guid tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!CanDesign(user) || tenantId == Guid.Empty || !CanAccessTenant(user, tenantId)) return Results.Forbid();
            var app = await db.Applications.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);
            if (app is null) return Results.NotFound();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Only draft applications can be edited.");

            var screen = await db.ApplicationScreens.SingleOrDefaultAsync(
                x => x.Id == screenId && x.ApplicationId == applicationId && x.TenantId == tenantId, token);
            if (screen is null) return Results.NotFound();

            var route = $"/builder/applications/{applicationId}/screens/{screen.Key}";
            var linkedModules = await db.ApplicationModules
                .Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId && x.ScreenId == screenId)
                .ToListAsync(token);
            var linkedModuleIds = linkedModules.Select(x => x.Id).ToArray();
            var menuItems = await db.ApplicationMenuItems
                .Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId &&
                    (x.Route == route || x.ModuleId.HasValue && linkedModuleIds.Contains(x.ModuleId.Value)))
                .ToListAsync(token);
            db.ApplicationMenuItems.RemoveRange(menuItems);
            foreach (var module in linkedModules)
            {
                module.ScreenId = null;
                module.UpdatedAt = DateTimeOffset.UtcNow;
            }
            db.ApplicationScreens.Remove(screen);
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        }).RequireAuthorization().WithTags("Application Screens").WithName("DeleteBuilderApplicationScreen");

        endpoints.MapPost("/applications/{applicationId:guid}/screens", async (
            Guid applicationId, SaveApplicationScreenRequest request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!CanDesign(user) || !CanAccessTenant(user, request.TenantId)) return Results.Forbid();
            if (!TryNormalizePageTitle(request.Title, out var title) || !ValidScreen(request, title))
                return Results.BadRequest("A valid screen key and page name are required; HTML must be under 100 KB.");
            var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token);
            if (app is null) return Results.NotFound();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Only draft applications can be edited.");
            var existingTitles = await db.ApplicationScreens
                .Where(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId)
                .Select(x => x.Title).ToListAsync(token);
            if (existingTitles.Any(existing => string.Equals(NormalizePageTitle(existing), title, StringComparison.OrdinalIgnoreCase)))
                return Results.Conflict(new { code = "page_name_exists", message = "A page with this name already exists in this application." });
            if (await db.ApplicationScreens.AnyAsync(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId && x.Key == request.Key, token))
                return Results.Conflict("A screen with this key already exists.");
            var screen = new BuilderApplicationScreen { ApplicationId = applicationId, TenantId = request.TenantId,
                Key = request.Key.Trim(), Title = title, HtmlContent = request.HtmlContent };
            db.ApplicationScreens.Add(screen);
            db.ApplicationMenuItems.Add(new BuilderApplicationMenuItem { ApplicationId = applicationId, TenantId = request.TenantId,
                Key = screen.Key, Label = screen.Title, Route = $"/builder/applications/{applicationId}/screens/{screen.Key}",
                SortOrder = await db.ApplicationMenuItems.CountAsync(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId, token) });
            await db.SaveChangesAsync(token);
            return Results.Ok(new { screen.Id, screen.Key, screen.Title, screen.HtmlContent, screen.ObjectId, screen.FormKey });
        }).RequireAuthorization().WithTags("Application Screens").WithName("CreateBuilderApplicationScreen");

        endpoints.MapPut("/applications/{applicationId:guid}/screens/{key}", async (
            Guid applicationId, string key, SaveApplicationScreenRequest request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!CanDesign(user) || !CanAccessTenant(user, request.TenantId)) return Results.Forbid();
            if (!TryNormalizePageTitle(request.Title, out var title) || !ValidScreen(request, title) || !string.Equals(key, request.Key, StringComparison.Ordinal))
                return Results.BadRequest("Screen key or page name is invalid.");
            var app = await db.Applications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token);
            if (app is null) return Results.NotFound();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Only draft applications can be edited.");
            var screen = await db.ApplicationScreens.SingleOrDefaultAsync(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId && x.Key == key, token);
            if (screen is null) return Results.NotFound();
            var existingTitles = await db.ApplicationScreens
                .Where(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId && x.Id != screen.Id)
                .Select(x => x.Title).ToListAsync(token);
            if (existingTitles.Any(existing => string.Equals(NormalizePageTitle(existing), title, StringComparison.OrdinalIgnoreCase)))
                return Results.Conflict(new { code = "page_name_exists", message = "A page with this name already exists in this application." });
            screen.Title = title; screen.HtmlContent = request.HtmlContent; screen.UpdatedAt = DateTimeOffset.UtcNow;
            var menu = await db.ApplicationMenuItems.SingleOrDefaultAsync(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId && x.Key == key, token);
            if (menu is not null) menu.Label = screen.Title;
            await db.SaveChangesAsync(token);
            return Results.Ok(new { screen.Id, screen.Key, screen.Title, screen.HtmlContent, screen.ObjectId, screen.FormKey });
        }).RequireAuthorization().WithTags("Application Screens").WithName("UpdateBuilderApplicationScreen");

        endpoints.MapGet("/applications/{applicationId:guid}/screens/{key}", async (
            Guid applicationId, string key, Guid tenantId, ClaimsPrincipal user, BuilderAuthorizationService authorization, BuilderDbContext db, CancellationToken token) =>
        {
            if (!CanDesign(user) || !CanAccessTenant(user, tenantId)) return Results.Forbid();
            var app = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);
            if (app is null || app.RecordState != RecordState.Active) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Draft screens are only available in the designer before publishing.");
            var screen = await db.ApplicationScreens.AsNoTracking()
                .Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId && x.Key == key)
                .Select(x => new { x.Id, x.Key, x.Title, x.HtmlContent, x.ObjectId, x.FormKey })
                .SingleOrDefaultAsync(token);
            if (screen is null) return Results.NotFound();
            if (screen.ObjectId is Guid objectId)
                await authorization.EnsureObjectAccess(tenantId, objectId, "read", user, token);
            return Results.Ok(new { screen.Id, screen.Key, screen.Title,
                screen.HtmlContent, screen.ObjectId, screen.FormKey, ApplicationName = app.Name, app.ThemeJson });
        }).RequireAuthorization().WithTags("Application Screens").WithName("GetBuilderApplicationScreen");

        endpoints.MapPut("/applications/{applicationId:guid}/theme", async (
            Guid applicationId, UpdateApplicationThemeRequest request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (user.Identity?.IsAuthenticated != true) return Results.Forbid();
            if (!CanAccessTenant(user, request.TenantId)) return Results.Forbid();
            var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token);
            if (app is null) return Results.NotFound();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Published applications must be copied to a draft before changing their theme.");
            try { app.ThemeJson = TemplateThemeService.Validate(request.ThemeJson); }
            catch (Exception ex) when (ex is ArgumentException or System.Text.Json.JsonException)
            { return Results.BadRequest(new { error = ex.Message }); }
            app.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return Results.Ok(new { app.Id, app.ThemeJson });
        }).WithTags("Application Screens").WithName("UpdateBuilderApplicationTheme");
    }

    private static bool CanDesign(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true;

    private static bool CanAccessTenant(ClaimsPrincipal user, Guid tenantId) =>
        user.IsInRole(BuilderRoleNames.SystemAdmin) ||
        (Guid.TryParse(user.FindFirst("companyId")?.Value ?? user.FindFirst("company_id")?.Value ?? user.FindFirst("tenant_id")?.Value, out var claimedTenant)
            && claimedTenant == tenantId);

    private static bool ValidScreen(SaveApplicationScreenRequest request, string normalizedTitle) =>
        request.TenantId != Guid.Empty && !string.IsNullOrWhiteSpace(normalizedTitle) && normalizedTitle.Length <= 120 &&
        !string.IsNullOrWhiteSpace(request.Key) && System.Text.RegularExpressions.Regex.IsMatch(request.Key, "^[a-z][a-z0-9-]{0,79}$") &&
        request.HtmlContent is { Length: <= 100_000 };

    private static bool TryNormalizePageTitle(string? value, out string title)
    {
        title = NormalizePageTitle(value);
        return title.Length is > 0 and <= 120 && System.Text.RegularExpressions.Regex.IsMatch(
            title, @"^[\p{L}\p{M}\p{N}][\p{L}\p{M}\p{N} '’&().,_-]*$",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
    }

    private static string NormalizePageTitle(string? value) =>
        string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

public sealed record UpdateApplicationThemeRequest(Guid TenantId, string ThemeJson);
public sealed record RenameApplicationScreenRequest(Guid TenantId, string Title);
public sealed record SaveApplicationScreenRequest(Guid TenantId, string Key, string Title, string HtmlContent);
