namespace SorteFederation.Builders.Endpoints;

public sealed class ApplicationModulesEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/application-modules").RequireAuthorization().WithTags("Application modules");
        group.MapPut("/{id:guid}/design", async (Guid id, BindApplicationModuleRequest request, BuilderDbContext db, CancellationToken token) =>
        {
            var module = await db.ApplicationModules.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == request.TenantId, token);
            if (module is null) return Results.NotFound();
            var app = await db.Applications.SingleAsync(x => x.Id == module.ApplicationId && x.TenantId == request.TenantId, token);
            if (app.Status == Data.Enums.DefinitionStatus.Published) return Results.Conflict("Create a draft before changing a published application's design.");
            var screen = await db.ApplicationScreens.SingleOrDefaultAsync(x => x.Id == request.ScreenId && x.ApplicationId == module.ApplicationId && x.TenantId == request.TenantId, token);
            var obj = await db.Objects.SingleOrDefaultAsync(x => x.Id == request.ObjectId && x.ApplicationId == module.ApplicationId && x.TenantId == request.TenantId, token);
            var form = await db.Forms.SingleOrDefaultAsync(x => x.Id == request.FormId && x.ObjectId == request.ObjectId && x.TenantId == request.TenantId, token);
            if (screen is null || obj is null || form is null) return Results.BadRequest("The screen, object and form must belong to this module's application and tenant.");
            module.ScreenId = screen.Id; module.ObjectId = obj.Id; module.FormId = form.Id;
            screen.ObjectId = obj.Id; screen.FormKey = form.Key;
            module.Status = Data.Enums.DefinitionStatus.Draft; module.Version++; module.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token); return Results.Ok(module);
        }).WithName("BindBuilderApplicationModuleDesign");
        group.MapGet("", async (Guid tenantId, Guid applicationId, BuilderDbContext db, CancellationToken token) =>
            Results.Ok(await db.ApplicationModules.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId).OrderBy(x => x.Name).ToListAsync(token)))
            .WithName("ListBuilderApplicationModules");
        group.MapPost("", async (CreateApplicationModuleRequest request, BuilderDbContext db, CancellationToken token) =>
        {
            if (!Valid(request.Key, request.Name)) return Results.BadRequest("A valid key and module name are required.");
            var app = await db.Applications.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == request.TenantId && x.Id == request.ApplicationId, token);
            if (app is null) return Results.NotFound();
            if (app.Status != Data.Enums.DefinitionStatus.Draft) return Results.Conflict("Only draft applications can add modules.");
            if (await db.ApplicationModules.AnyAsync(x => x.TenantId == request.TenantId && x.ApplicationId == request.ApplicationId && x.Key == request.Key, token)) return Results.Conflict("Module key already exists.");
            var module = new BuilderApplicationModule { TenantId = request.TenantId, ApplicationId = request.ApplicationId, Key = request.Key, Name = request.Name.Trim(), Description = request.Description?.Trim() };
            db.ApplicationModules.Add(module); await db.SaveChangesAsync(token);
            return Results.Created($"/application-modules/{module.Id}", module);
        }).WithName("CreateBuilderApplicationModule");
        group.MapPut("/{id:guid}", async (Guid id, UpdateApplicationModuleRequest request, BuilderDbContext db, CancellationToken token) =>
        {
            if (!Valid(request.Key, request.Name)) return Results.BadRequest("A valid key and module name are required.");
            var module = await db.ApplicationModules.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == request.TenantId, token);
            if (module is null) return Results.NotFound();
            if (await db.ApplicationModules.AnyAsync(x => x.TenantId == module.TenantId && x.ApplicationId == module.ApplicationId && x.Id != id && x.Key == request.Key, token)) return Results.Conflict("Module key already exists.");
            module.Key = request.Key; module.Name = request.Name.Trim(); module.Description = request.Description?.Trim(); module.Version++; module.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token); return Results.Ok(module);
        }).WithName("UpdateBuilderApplicationModule");
        group.MapDelete("/{id:guid}", async (Guid id, Guid tenantId, BuilderDbContext db, CancellationToken token) =>
        {
            var module = await db.ApplicationModules.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, token);
            if (module is null) return Results.NotFound();
            if (await db.ApplicationMenuItems.AnyAsync(x => x.TenantId == tenantId && x.ModuleId == id, token)) return Results.Conflict("Remove or reassign this module's menu items before deleting it.");
            db.ApplicationModules.Remove(module); await db.SaveChangesAsync(token); return Results.NoContent();
        }).WithName("DeleteBuilderApplicationModule");
    }

    private static bool Valid(string key, string name) => !string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(name)
        && name.Length <= 120 && System.Text.RegularExpressions.Regex.IsMatch(key, "^[a-z][a-z0-9-]{0,79}$");
}
