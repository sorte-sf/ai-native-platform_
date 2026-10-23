namespace SorteFederation.Builders.Endpoints;

public sealed record SavePageDefinitionRequest(Guid TenantId, string DefinitionJson);
public sealed class PageDefinitionsEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/applications/{applicationId:guid}/pages/{pageId:guid}/definition", async (
            Guid applicationId, Guid pageId,
            SavePageDefinitionRequest request,
            BuilderDbContext db, CancellationToken token) =>
        { 
            var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token);

            if (app is null) return Results.NotFound();

            if (app.Status != Data.Enums.DefinitionStatus.Draft) return Results.Conflict("Only draft pages can be edited.");

            var page = await db.ApplicationScreens.SingleOrDefaultAsync(x => x.Id == pageId && x.ApplicationId == applicationId && x.TenantId == request.TenantId, token);

            if (page is null) return Results.NotFound();

            await new PageDefinitionService(db).Validate(request.TenantId, applicationId, request.DefinitionJson, token);

            page.DefinitionJson = request.DefinitionJson; page.Version++; page.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(token); return Results.Ok(new { page.Id, page.DefinitionJson });

        }).RequireAuthorization().WithName("SaveBuilderPageDefinition").WithTags("Pages");
    }
}
