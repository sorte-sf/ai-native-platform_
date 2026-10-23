
namespace SorteFederation.Builders.Endpoints;

public sealed class TemplatesEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapTemplatesEndpoints(endpoints);

    public static IEndpointRouteBuilder MapTemplatesEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/templates").RequireAuthorization().WithTags("Templates");

        group.MapGet("", async (BuilderDbContext db, CancellationToken token) =>
            Results.Ok(await db.Templates.AsNoTracking()
                .Where(x => x.RecordState == RecordState.Active && !x.Category.StartsWith("Job Schedule:"))
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Key, x.Name, x.Description, x.Category, x.BlueprintVersion, x.PreviewImageUrl, x.HtmlContent, x.ScssContent, x.ManifestJson, x.ManifestVersion, x.IsBuiltIn, x.BaseTemplate, x.ThemeJson })
                .ToListAsync(token)));

        group.MapGet("/{id:guid}", async (Guid id, BuilderDbContext db, CancellationToken token) =>
            await db.Templates.AsNoTracking().Where(x => x.Id == id && x.RecordState == RecordState.Active).Select(x => new { x.Id, x.Key, x.Name, x.Description, x.Category, x.BlueprintVersion, x.PreviewImageUrl, x.HtmlContent, x.ScssContent, x.ManifestJson, x.ManifestVersion, x.IsBuiltIn, x.BaseTemplate, x.ThemeJson }).SingleOrDefaultAsync(token) is { } item ? Results.Ok(item) : Results.NotFound());

        group.MapGet("/{id:guid}/releases", async (Guid id, BuilderDbContext db, CancellationToken token) =>
            Results.Ok((await db.TemplateReleases.AsNoTracking().Where(x => x.TemplateId == id).ToListAsync(token))
                .OrderByDescending(x => x, TemplateReleaseService.VersionComparer)
                .Select(x => new { x.Id, x.TemplateId, x.ReleaseVersion, x.ManifestVersion, x.Description, x.PreviewImageUrl, x.BaseTemplate, x.ReleasedAt })));

        group.MapGet("/{id:guid}/releases/{releaseId:guid}", async (Guid id, Guid releaseId, BuilderDbContext db, CancellationToken token) =>
            await db.TemplateReleases.AsNoTracking().Where(x => x.TemplateId == id && x.Id == releaseId)
                .Select(x => new { x.Id, x.TemplateId, x.ReleaseVersion, x.ManifestVersion, x.ManifestJson, x.ThemeJson, x.HtmlContent, x.ScssContent, x.Description, x.PreviewImageUrl, x.BaseTemplate, x.ReleasedAt })
                .SingleOrDefaultAsync(token) is { } release ? Results.Ok(release) : Results.NotFound());

        group.MapPost("", async (CreateTemplateRequest request, BuilderDbContext db, CancellationToken token) =>
        {
            var manifestJson = string.IsNullOrWhiteSpace(request.ManifestJson) ? null : request.ManifestJson;
            var manifest = manifestJson is null ? null : TemplateManifestService.Parse(manifestJson);
            var entity = new BuilderTemplate
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                HtmlContent = request.HtmlContent,
                ScssContent = request.ScssContent,
                ManifestJson = manifestJson,
                ManifestVersion = manifest?.Version ?? 1,
                BlueprintVersion = "1.0.0",
                BaseTemplate = "fuse-angular-22",
                ThemeJson = string.IsNullOrWhiteSpace(request.ThemeJson) ? null : TemplateThemeService.Validate(request.ThemeJson)
            };
            db.Templates.Add(entity);
            var release = await TemplateReleaseService.EnsureReleaseAsync(db, entity, token);
            await db.SaveChangesAsync(token);
            return Results.Created($"/templates/{entity.Id}", new { entity.Id, entity.Name, entity.BlueprintVersion, CurrentReleaseId = release.Id });
        }).RequireAuthorization(policy => policy.RequireRole(BuilderRoleNames.SystemAdmin));

        group.MapPut("/{id:guid}", async (Guid id, UpdateTemplateRequest request, BuilderDbContext db, CancellationToken token) =>
        {
            var manifestJson = string.IsNullOrWhiteSpace(request.ManifestJson) ? null : request.ManifestJson;
            var manifest = manifestJson is null ? null : TemplateManifestService.Parse(manifestJson);
            var entity = await db.Templates.SingleOrDefaultAsync(x => x.Id == id, token);

            if (entity is null) return Results.NotFound();
            if (entity.RecordState != RecordState.Active) return Results.Conflict("Archived templates cannot be edited.");
            if (entity.IsBuiltIn) return Results.Conflict("Built-in templates cannot be edited. Create a new template instead.");

            entity.Name = request.Name.Trim(); entity.Description = request.Description?.Trim(); entity.HtmlContent = request.HtmlContent; entity.ScssContent = request.ScssContent;
            entity.ManifestJson = manifestJson; entity.ManifestVersion = manifest?.Version ?? 1;
            entity.BlueprintVersion = TemplateReleaseService.NextPatch(entity.BlueprintVersion);
            entity.BaseTemplate = "fuse-angular-22";
            entity.ThemeJson = string.IsNullOrWhiteSpace(request.ThemeJson) ? null : TemplateThemeService.Validate(request.ThemeJson);

            entity.UpdatedAt = DateTimeOffset.UtcNow; entity.Version++;
            var release = await TemplateReleaseService.EnsureReleaseAsync(db, entity, token);
            await db.SaveChangesAsync(token);
            return Results.Ok(new { entity.Id, entity.Name, entity.BlueprintVersion, CurrentReleaseId = release.Id });
        }).RequireAuthorization(policy => policy.RequireRole(BuilderRoleNames.SystemAdmin));

        group.MapDelete("/{id:guid}", async (Guid id, BuilderDbContext db, CancellationToken token) =>
        {
            var entity = await db.Templates.SingleOrDefaultAsync(x => x.Id == id, token);
            if (entity is null) return Results.NotFound();
            if (entity.IsBuiltIn) return Results.Conflict("Built-in templates cannot be deleted.");
            if (entity.RecordState != RecordState.Active) return Results.NoContent();
            entity.RecordState = RecordState.Deleted;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.Version++;
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole(BuilderRoleNames.SystemAdmin));

        return endpoints;
    }
}
