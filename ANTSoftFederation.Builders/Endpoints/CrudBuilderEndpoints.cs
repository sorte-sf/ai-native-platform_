namespace SorteFederation.Builders.Endpoints;

public sealed record GenerateCrudRequest(Guid TenantId, Guid ObjectId);

public sealed class CrudBuilderEndpoints : IAppEndPoints
{
    internal static bool CanManage(ClaimsPrincipal user, Guid tenantId) => tenantId != Guid.Empty &&
        (user.IsInRole(BuilderRoleNames.SystemAdmin) ||
        (user.Identity?.IsAuthenticated == true &&
        Guid.TryParse(user.FindFirst("companyId")?.Value ?? user.FindFirst("company_id")?.Value ?? user.FindFirst("tenant_id")?.Value, out var tenant) && tenant == tenantId));

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/applications/{applicationId:guid}/data-objects", async (Guid applicationId, Guid tenantId, ClaimsPrincipal user, FormDesignerService service, CancellationToken token) =>
        {
            if (!CanManage(user, tenantId)) return Results.Forbid();
            var app = await service.GetApplication(tenantId, applicationId, token);
            return app is null ? Results.NotFound() : Results.Ok(app);
        }).RequireAuthorization().WithTags("CRUD Builder");

        endpoints.MapPost("/applications/{applicationId:guid}/data-objects", async (Guid applicationId, SaveDesignerDraftRequest request, ClaimsPrincipal user, FormDesignerService service, CancellationToken token) =>
        {
            if (!CanManage(user, request.TenantId)) return Results.Forbid();
            if (request.Fields is null || request.Fields.Count is < 1 or > 100 || request.Fields.Any(field => field.Id is not null || !System.Text.RegularExpressions.Regex.IsMatch(field.Key ?? "", "^[a-z][a-z0-9_]{0,79}$"))) return Results.BadRequest("New objects need between 1 and 100 fields with valid keys and no existing field IDs.");
            return Results.Ok(await service.SaveDraft(request with { ApplicationId = applicationId, ObjectId = null, FormId = null, PageId = null, PageNodeId = null }, token));
        }).RequireAuthorization().WithTags("CRUD Builder");

        endpoints.MapPost("/applications/{applicationId:guid}/generate-crud", async (Guid applicationId, GenerateCrudRequest request, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!CanManage(user, request.TenantId)) return Results.Forbid();
            var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token);
            if (app is null) return Results.NotFound();
            if (app.Status != DefinitionStatus.Draft) return Results.Conflict("Only draft applications can generate screens.");
            var obj = await db.Objects.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.ObjectId && x.ApplicationId == applicationId && x.TenantId == request.TenantId, token);
            if (obj is null) return Results.NotFound();
            var form = await db.Forms.AsNoTracking().Where(x => x.ObjectId == obj.Id && x.TenantId == request.TenantId).OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync(token);
            if (form is null) return Results.BadRequest("Create a form for this data object first.");
            var fields = await db.Fields.AsNoTracking().Where(x => x.ObjectId == obj.Id && x.TenantId == request.TenantId).OrderBy(x => x.SortOrder).Take(50).ToListAsync(token);
            if (fields.Count == 0) return Results.BadRequest("Add fields before generating CRUD.");
            var key = $"crud-{obj.Id:N}";
            if (await db.ApplicationScreens.AnyAsync(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId && x.Key == key, token)) return Results.Conflict("CRUD screen already exists. Open it in the designer.");
            var definition = new PageDefinition(1, [new PageNode(Guid.NewGuid().ToString(), "crud", obj.Name, obj.Id, form.Id, Columns: fields.Select(field => field.Key).ToList())]);
            var json = JsonSerializer.Serialize(definition, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await new PageDefinitionService(db).Validate(request.TenantId, applicationId, json, token);
            var screen = new BuilderApplicationScreen { TenantId = request.TenantId, ApplicationId = applicationId, Key = key, Title = obj.Name,
                HtmlContent = "", ObjectId = obj.Id, FormId = form.Id, FormKey = form.Key, DefinitionJson = json };
            db.ApplicationScreens.Add(screen);
            var module = await db.ApplicationModules.SingleOrDefaultAsync(x => x.TenantId == request.TenantId && x.ApplicationId == applicationId && x.ObjectId == obj.Id, token);
            if (module is not null && module.ScreenId is null) { module.ScreenId = screen.Id; module.FormId = form.Id; module.UpdatedAt = DateTimeOffset.UtcNow; }
            db.ApplicationMenuItems.Add(new BuilderApplicationMenuItem { TenantId = request.TenantId, ApplicationId = applicationId,
                Key = key, Label = obj.Name, Route = $"/builder/applications/{applicationId}/screens/{key}", Visible = true,
                ModuleId = module?.Id,
                SortOrder = await db.ApplicationMenuItems.CountAsync(x => x.TenantId == request.TenantId && x.ApplicationId == applicationId, token) });
            await db.SaveChangesAsync(token);
            return Results.Ok(new { screen.Id, screen.Key, screen.Title });
        }).RequireAuthorization().WithTags("CRUD Builder");
    }
}
