using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class PublishingEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapPublishingEndpoints(endpoints);

public static IEndpointRouteBuilder MapPublishingEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/applications/{applicationId:guid}/draft-preview", async (Guid applicationId, Guid tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) => {
            if (!CrudBuilderEndpoints.CanManage(user, tenantId)) return Results.Forbid();
            var app = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status != Data.Enums.DefinitionStatus.Draft) return Results.Conflict("Draft preview is only available before publishing.");
            var objects = await db.Objects.AsNoTracking().Include(x => x.Fields).Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId).ToListAsync(token);
            var ids = objects.Select(x => x.Id).ToArray();
            var bundle = new {
                app.Id, app.Name, app.ThemeJson, app.PublishedVersion,
                Modules = await db.ApplicationModules.AsNoTracking().Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId).Select(x => new { x.Id, x.ScreenId, x.ObjectId, x.FormId }).ToListAsync(token),
                Screens = await db.ApplicationScreens.AsNoTracking().Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId).Select(x => new { x.Id, x.Key, x.Title, x.HtmlContent, x.ObjectId, x.FormId, x.FormKey, x.DefinitionJson }).ToListAsync(token),
                Menus = await db.ApplicationMenuItems.AsNoTracking().Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId).Select(x => new { x.Id, x.ParentId, x.Label, x.Icon, x.PermissionKey, x.Route, x.SortOrder, x.Visible }).ToListAsync(token),
                Forms = await db.Forms.AsNoTracking().Where(x => ids.Contains(x.ObjectId) && x.TenantId == tenantId).Select(x => new { x.Id, x.ObjectId, x.Key, x.Name, x.LayoutJson }).ToListAsync(token),
                Objects = objects.Select(x => new { x.Id, Fields = x.Fields.Select(f => new { f.Key, f.Label, f.Type, f.SortOrder, f.Required, f.ReadOnly, f.ConfigurationJson }) })
            };
            // Match the published snapshot contract, including enum names and casing.
            return Results.Ok(JsonSerializer.SerializeToElement(bundle, new JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }));
        }).RequireAuthorization().WithTags("Publishing");
        endpoints.MapPost("/applications/{applicationId:guid}/validate", async (Guid applicationId, PublishApplicationRequest request, ClaimsPrincipal user, BuilderDbContext db, BuilderMetadataService service, CancellationToken token) => {
            if (!CrudBuilderEndpoints.CanManage(user, request.TenantId)) return Results.Forbid();
            var app = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status != Data.Enums.DefinitionStatus.Draft) return Results.Conflict("Only drafts can be validated for publishing.");
            return Results.Ok(await service.ValidateApplication(request.TenantId, applicationId, token));
        }).RequireAuthorization().WithTags("Publishing");

        endpoints.MapPost("/applications/{applicationId:guid}/publish", async (Guid applicationId, PublishApplicationRequest request, ClaimsPrincipal user, BuilderDbContext db, BuilderMetadataService service, PublishedApplicationDraftService drafts, CancellationToken token) => {
            if (!CrudBuilderEndpoints.CanManage(user, request.TenantId)) return Results.Forbid();
            var app = await db.Applications.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(user, app)) return Results.Forbid();
            if (app.Status != Data.Enums.DefinitionStatus.Draft) return Results.Conflict("Only draft applications can be published.");
            try
            {
                if (await drafts.IsPublishedForkDraftAsync(applicationId, request.TenantId, token))
                    return Results.Ok(await drafts.PublishDraftAsync(applicationId, request.TenantId, ApplicationEditingPolicy.UserId(user), token));
                return Results.Ok(await service.PublishApplication(request with { ApplicationId = applicationId }, token));
            }
            catch (InvalidOperationException ex) { return Results.Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
        }).RequireAuthorization().WithTags("Publishing");

        return endpoints;
    }
}
