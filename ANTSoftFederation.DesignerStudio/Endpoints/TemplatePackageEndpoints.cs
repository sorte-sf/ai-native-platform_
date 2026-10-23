using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using SorteFederation.DesignerStudio.Data;
using SorteFederation.DesignerStudio.Data.Entities;
using SorteFederation.DesignerStudio.Storage;
using SorteFederation.Shared.TemplateCatalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.DesignerStudio.Endpoints;

public static partial class TemplatePackageEndpoints
{
    public const long MaximumPackageBytes = FileSystemTemplatePackageStorage.MaximumPackageBytes;

    public static void Map(RouteGroupBuilder api)
    {
        var catalog = api.MapGroup("/templates").WithTags("Template catalog");
        catalog.MapGet("", async (DesignerStudioDbContext db, CancellationToken token) =>
        {
            var templates = await db.TemplatePackages.AsNoTracking()
                .Where(x => x.Releases.Any(release => release.Status == TemplateReleaseStatus.Published))
                .Include(x => x.Releases.Where(release => release.Status == TemplateReleaseStatus.Published))
                .OrderBy(x => x.Category).ThenBy(x => x.Name)
                .ToListAsync(token);

            return Results.Ok(templates.Select(template =>
            {
                var latest = template.Releases.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Version).First();
                return CatalogItem(template, latest);
            }));
        });

        catalog.MapGet("/{key}", async (string key, DesignerStudioDbContext db, CancellationToken token) =>
        {
            var template = await db.TemplatePackages.AsNoTracking()
                .Include(x => x.Releases.Where(release => release.Status == TemplateReleaseStatus.Published))
                .SingleOrDefaultAsync(x => x.Key == key, token);
            if (template is null || template.Releases.Count == 0) return Results.NotFound();
            return Results.Ok(CatalogItem(template, template.Releases.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Version).First()));
        });

        catalog.MapGet("/{key}/{version}", async (string key, string version, DesignerStudioDbContext db, CancellationToken token) =>
        {
            var release = await db.TemplatePackageReleases.AsNoTracking()
                .Include(x => x.TemplatePackage)
                .SingleOrDefaultAsync(x => x.TemplatePackage.Key == key && x.Version == version && x.Status == TemplateReleaseStatus.Published, token);
            return release is null ? Results.NotFound() : Results.Ok(CatalogItem(release.TemplatePackage, release));
        });

        catalog.MapGet("/{key}/{version}/package", async (string key, string version, DesignerStudioDbContext db, ITemplatePackageStorage storage, CancellationToken token) =>
        {
            var release = await db.TemplatePackageReleases.AsNoTracking()
                .Include(x => x.TemplatePackage)
                .SingleOrDefaultAsync(x => x.TemplatePackage.Key == key && x.Version == version && x.Status == TemplateReleaseStatus.Published, token);
            if (release is null) return Results.NotFound();
            var stream = storage.OpenRead(release.PackagePath);
            return Results.File(stream, "application/vnd.antsoft.template-package", $"{key}-{version}.anttemplate", enableRangeProcessing: true);
        });

        var studio = api.MapGroup("/studio/templates")
            .WithTags("Template authoring")
            .RequireAuthorization("DesignerStudioAccess");
        studio.MapGet("", async (ClaimsPrincipal user, DesignerStudioDbContext db, CancellationToken token) =>
        {
            var owner = UserId(user);
            var items = await db.TemplatePackages.AsNoTracking().Where(x => x.OwnerUserId == owner)
                .Include(x => x.Releases).ToListAsync(token);
            return Results.Ok(items.Select(x => new
            {
                x.Id, x.Key, x.Name, x.Category, x.Description, x.CreatedAt, x.UpdatedAt,
                Releases = x.Releases.OrderByDescending(release => release.CreatedAt).Select(ReleaseItem)
            }).OrderByDescending(x => x.UpdatedAt));
        });

        studio.MapPost("/figma/import", async (FigmaImportRequest request, FigmaImportService importer, CancellationToken token) =>
        {
            try
            {
                var result = await importer.ImportAsync(request.FileUrl, request.AccessToken, request.NodeId, token);
                return Results.Ok(result);
            }
            catch (FigmaImportException ex)
            {
                return Results.Problem(statusCode: ex.StatusCode, title: "Figma import failed.", detail: ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Check the Figma URL and token.", detail: ex.Message);
            }
        }).RequireAuthorization("DesignerStudioAccess");

        studio.MapPost("", async (CreateTemplatePackageRequest request, ClaimsPrincipal user, DesignerStudioDbContext db, CancellationToken token) =>
        {
            var key = NormalizeKey(request.Key);
            var name = Required(request.Name, "Template name is required.", 160);
            var category = Required(request.Category, "Template category is required.", 80);
            var description = Optional(request.Description, 2000);
            if (await db.TemplatePackages.AnyAsync(x => x.Key == key, token)) return Results.Conflict("That template key is already in use.");

            var package = new TemplatePackage
            {
                Key = key,
                Name = name,
                Category = category,
                Description = description,
                OwnerUserId = UserId(user)
            };
            db.TemplatePackages.Add(package);
            await db.SaveChangesAsync(token);
            return Results.Created($"/api/v1/studio/templates/{package.Id}", new { package.Id, package.Key, package.Name, package.Category, package.Description });
        });

        studio.MapPut("/{templateId:guid}", async (Guid templateId, UpdateTemplatePackageRequest request, ClaimsPrincipal user, DesignerStudioDbContext db, CancellationToken token) =>
        {
            var package = await db.TemplatePackages.SingleOrDefaultAsync(x => x.Id == templateId && x.OwnerUserId == UserId(user), token);
            if (package is null) return Results.NotFound();
            package.Name = Required(request.Name, "Template name is required.", 160);
            package.Category = Required(request.Category, "Template category is required.", 80);
            package.Description = Optional(request.Description, 2000);
            package.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return Results.Ok(new { package.Id, package.Key, package.Name, package.Category, package.Description, package.UpdatedAt });
        });

        studio.MapGet("/{templateId:guid}/draft", async (Guid templateId, ClaimsPrincipal user, DesignerStudioDbContext db, CancellationToken token) =>
        {
            var definition = await db.TemplatePackages.AsNoTracking()
                .Where(x => x.Id == templateId && x.OwnerUserId == UserId(user))
                .Select(x => x.DraftDefinitionJson)
                .SingleOrDefaultAsync(token);
            return definition is null ? Results.NotFound() : Results.Content(definition, "application/json");
        });

        studio.MapPut("/{templateId:guid}/draft", async (
            Guid templateId,
            SaveTemplateDraftRequest request,
            ClaimsPrincipal user,
            DesignerStudioDbContext db,
            CancellationToken token) =>
        {
            var package = await db.TemplatePackages.SingleOrDefaultAsync(x => x.Id == templateId && x.OwnerUserId == UserId(user), token);
            if (package is null) return Results.NotFound();
            if (request.Definition.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("A template draft must be a JSON object.");

            var definition = request.Definition.GetRawText();
            if (System.Text.Encoding.UTF8.GetByteCount(definition) > 1_000_000)
                throw new ArgumentException("A template draft cannot exceed 1 MB.");

            package.DraftDefinitionJson = definition;
            package.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return Results.Content(package.DraftDefinitionJson, "application/json");
        });

        studio.MapPost("/{templateId:guid}/releases/generate", async (
            Guid templateId,
            GenerateTemplateReleaseRequest request,
            ClaimsPrincipal user,
            DesignerStudioDbContext db,
            ITemplatePackageStorage storage,
            CancellationToken token) =>
        {
            var package = await db.TemplatePackages.SingleOrDefaultAsync(x => x.Id == templateId && x.OwnerUserId == UserId(user), token);
            if (package is null) return Results.NotFound();
            var version = RequiredVersion(request.Version);
            if (package.DraftDefinitionJson is null) return Results.Conflict("Save the template design before generating a release.");
            if (await db.TemplatePackageReleases.AnyAsync(x => x.TemplatePackageId == templateId && x.Version == version, token))
                return Results.Conflict("A release with that version already exists. Create a new version; releases are immutable.");

            var bytes = DraftTemplatePackageGenerator.Generate(package.Key, package.Name, version, package.DraftDefinitionJson);
            await using var stream = new MemoryStream(bytes, writable: false);
            var file = new FormFile(stream, 0, stream.Length, "packageFile", $"{package.Key}-{version}.anttemplate");
            var releaseId = Guid.CreateVersion7();
            var stored = await storage.StoreAsync(file, releaseId, package.Key, package.Name, version, token);
            try
            {
                var release = new TemplatePackageRelease
                {
                    Id = releaseId,
                    TemplatePackageId = package.Id,
                    Version = version,
                    ManifestJson = stored.ManifestJson,
                    PackagePath = stored.Path,
                    Sha256 = stored.Sha256,
                    SizeBytes = stored.SizeBytes,
                    ChangeNotes = Optional(request.ChangeNotes, 4000),
                    CreatedByUserId = UserId(user)
                };
                db.TemplatePackageReleases.Add(release);
                package.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(token);
                return Results.Created($"/api/v1/studio/releases/{release.Id}", ReleaseItem(release));
            }
            catch
            {
                storage.Delete(stored.Path);
                throw;
            }
        });

        studio.MapPost("/{templateId:guid}/releases", async (
            Guid templateId,
            [FromForm] string version,
            [FromForm] string? changeNotes,
            [FromForm] IFormFile packageFile,
            ClaimsPrincipal user,
            DesignerStudioDbContext db,
            ITemplatePackageStorage storage,
            CancellationToken token) =>
        {
            var package = await db.TemplatePackages.SingleOrDefaultAsync(x => x.Id == templateId && x.OwnerUserId == UserId(user), token);
            if (package is null) return Results.NotFound();
            version = RequiredVersion(version);
            if (await db.TemplatePackageReleases.AnyAsync(x => x.TemplatePackageId == templateId && x.Version == version, token))
                return Results.Conflict("A release with that version already exists. Create a new version; releases are immutable.");

            var releaseId = Guid.CreateVersion7();
            var stored = await storage.StoreAsync(packageFile, releaseId, package.Key, package.Name, version, token);
            try
            {
                var release = new TemplatePackageRelease
                {
                    Id = releaseId,
                    TemplatePackageId = package.Id,
                    Version = version,
                    ManifestJson = stored.ManifestJson,
                    PackagePath = stored.Path,
                    Sha256 = stored.Sha256,
                    SizeBytes = stored.SizeBytes,
                    ChangeNotes = Optional(changeNotes, 4000),
                    CreatedByUserId = UserId(user)
                };
                db.TemplatePackageReleases.Add(release);
                package.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(token);
                return Results.Created($"/api/v1/studio/releases/{release.Id}", ReleaseItem(release));
            }
            catch
            {
                storage.Delete(stored.Path);
                throw;
            }
        }).DisableAntiforgery();

        var studioReleases = api.MapGroup("/studio/releases")
            .WithTags("Template authoring")
            .RequireAuthorization("DesignerStudioAccess");
        studioReleases.MapPut("/{releaseId:guid}/package", async (
            Guid releaseId,
            [FromForm] IFormFile packageFile,
            [FromForm] string? changeNotes,
            ClaimsPrincipal user,
            DesignerStudioDbContext db,
            ITemplatePackageStorage storage,
            CancellationToken token) =>
        {
            var release = await db.TemplatePackageReleases.Include(x => x.TemplatePackage)
                .SingleOrDefaultAsync(x => x.Id == releaseId && x.TemplatePackage.OwnerUserId == UserId(user), token);
            if (release is null) return Results.NotFound();
            if (release.Status is not (TemplateReleaseStatus.Draft or TemplateReleaseStatus.Rejected))
                return Results.Conflict("Submitted or published releases cannot be replaced. Create a new version instead.");

            var stored = await storage.StoreAsync(packageFile, release.Id, release.TemplatePackage.Key, release.TemplatePackage.Name, release.Version, token);
            var previousPath = release.PackagePath;
            try
            {
                release.ManifestJson = stored.ManifestJson;
                release.PackagePath = stored.Path;
                release.Sha256 = stored.Sha256;
                release.SizeBytes = stored.SizeBytes;
                release.ChangeNotes = Optional(changeNotes, 4000);
                release.Status = TemplateReleaseStatus.Draft;
                release.SubmittedAt = null;
                release.ReviewedAt = null;
                release.ReviewNotes = null;
                release.ReviewedByUserId = null;
                await db.SaveChangesAsync(token);
            }
            catch
            {
                storage.Delete(stored.Path);
                throw;
            }
            storage.Delete(previousPath);
            return Results.Ok(ReleaseItem(release));
        }).DisableAntiforgery();

        studioReleases.MapPost("/{releaseId:guid}/submit", async (
            Guid releaseId,
            ClaimsPrincipal user,
            DesignerStudioDbContext db,
            ITemplateCatalogEventBus eventBus,
            ILogger<TemplateReleasePublishedEvent> logger,
            CancellationToken token) =>
        {
            var release = await db.TemplatePackageReleases.Include(x => x.TemplatePackage)
                .SingleOrDefaultAsync(x => x.Id == releaseId && x.TemplatePackage.OwnerUserId == UserId(user), token);
            if (release is null) return Results.NotFound();
            if (release.Status is not (TemplateReleaseStatus.Draft or TemplateReleaseStatus.Rejected))
                return Results.Conflict("Only a draft or rejected release can be submitted for review.");

            var submittedAt = DateTimeOffset.UtcNow;
            var isSystemAdmin = user.IsInRole("SystemAdmin");
            release.Status = isSystemAdmin ? TemplateReleaseStatus.Published : TemplateReleaseStatus.Submitted;
            release.SubmittedAt = submittedAt;
            release.PublishedAt = isSystemAdmin ? submittedAt : null;
            release.ReviewedAt = null;
            release.ReviewNotes = null;
            release.ReviewedByUserId = null;
            release.TemplatePackage.UpdatedAt = submittedAt;
            await db.SaveChangesAsync(token);
            if (release.Status == TemplateReleaseStatus.Published)
                await NotifyPublishedAsync(eventBus, logger, release, token);
            return Results.Ok(ReleaseItem(release));
        });

        var review = api.MapGroup("/review/releases").WithTags("Template review").RequireAuthorization("TemplateReview");
        review.MapGet("", async (DesignerStudioDbContext db, CancellationToken token) =>
        {
            var releases = await db.TemplatePackageReleases.AsNoTracking()
                .Include(x => x.TemplatePackage)
                .Where(x => x.Status == TemplateReleaseStatus.Submitted)
                .ToListAsync(token);

            return Results.Ok(releases.OrderBy(release => release.SubmittedAt).Select(release => new
            {
                Template = new { release.TemplatePackage.Id, release.TemplatePackage.Key, release.TemplatePackage.Name, release.TemplatePackage.Category },
                Release = ReleaseItem(release)
            }));
        });

        review.MapPost("/{releaseId:guid}/reject", async (
            Guid releaseId,
            RejectTemplateReleaseRequest request,
            ClaimsPrincipal user,
            DesignerStudioDbContext db,
            CancellationToken token) =>
        {
            var release = await db.TemplatePackageReleases.Include(x => x.TemplatePackage)
                .SingleOrDefaultAsync(x => x.Id == releaseId, token);
            if (release is null) return Results.NotFound();
            if (release.Status != TemplateReleaseStatus.Submitted)
                return Results.Conflict("Only a submitted release can be rejected.");

            release.Status = TemplateReleaseStatus.Rejected;
            release.ReviewedAt = DateTimeOffset.UtcNow;
            release.ReviewedByUserId = UserId(user);
            release.ReviewNotes = Required(request.ReviewNotes, "A reason is required when rejecting a release.", 4000);
            await db.SaveChangesAsync(token);
            return Results.Ok(ReleaseItem(release));
        });

        review.MapPost("/{releaseId:guid}/publish", async (
            Guid releaseId,
            ClaimsPrincipal user,
            DesignerStudioDbContext db,
            ITemplateCatalogEventBus eventBus,
            ILogger<TemplateReleasePublishedEvent> logger,
            CancellationToken token) =>
        {
            var release = await db.TemplatePackageReleases.Include(x => x.TemplatePackage).SingleOrDefaultAsync(x => x.Id == releaseId, token);
            if (release is null) return Results.NotFound();
            if (release.Status != TemplateReleaseStatus.Submitted)
                return Results.Conflict("Only a submitted release can be published.");
            release.Status = TemplateReleaseStatus.Published;
            release.PublishedAt = DateTimeOffset.UtcNow;
            release.ReviewedAt = release.PublishedAt;
            release.ReviewNotes = null;
            release.ReviewedByUserId = UserId(user);
            release.TemplatePackage.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            await NotifyPublishedAsync(eventBus, logger, release, token);
            return Results.Ok(ReleaseItem(release));
        });
    }

    private static async Task NotifyPublishedAsync(
        ITemplateCatalogEventBus eventBus,
        ILogger logger,
        TemplatePackageRelease release,
        CancellationToken token)
    {
        try
        {
            var template = release.TemplatePackage;
            await eventBus.PublishAsync(new TemplateReleasePublishedEvent(
                template.Id,
                template.Key,
                template.Name,
                template.Category,
                template.Description,
                release.Id,
                release.Version,
                release.PublishedAt ?? DateTimeOffset.UtcNow,
                release.ManifestJson), token);
        }
        catch (Exception exception) when (!token.IsCancellationRequested)
        {
            // Builder's periodic reconciliation recovers if RabbitMQ is temporarily unavailable.
            logger.LogError(exception, "Template release {ReleaseId} was published, but its catalog event could not be sent.", release.Id);
        }
    }

    private static object CatalogItem(TemplatePackage package, TemplatePackageRelease release) => new
    {
        TemplateId = package.Id,
        package.Key,
        package.Name,
        package.Category,
        package.Description,
        ReleaseId = release.Id,
        release.Version,
        release.ManifestJson,
        release.Sha256,
        release.SizeBytes,
        release.ChangeNotes,
        release.PublishedAt
    };

    private static object ReleaseItem(TemplatePackageRelease release) => new
    {
        release.Id,
        release.TemplatePackageId,
        release.Version,
        Status = release.Status.ToString(),
        release.ChangeNotes,
        release.Sha256,
        release.SizeBytes,
        release.CreatedAt,
        release.SubmittedAt,
        release.ReviewedAt,
        release.ReviewNotes,
        release.PublishedAt
    };

    private static string UserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? user.FindFirstValue("user_id")
        ?? throw new UnauthorizedAccessException("The login token does not identify a user.");

    private static string NormalizeKey(string? value)
    {
        var key = Required(value, "Template key is required.", 100).ToLowerInvariant();
        if (!KeyPattern().IsMatch(key)) throw new ArgumentException("Template key must start with a letter and contain only lowercase letters, numbers, and hyphens.");
        return key;
    }

    private static string RequiredVersion(string? value)
    {
        var version = Required(value, "A semantic release version is required (for example 1.0.0).", 32);
        if (!VersionPattern().IsMatch(version)) throw new ArgumentException("Version must follow semantic versioning, for example 1.0.0 or 1.2.0-beta.1.");
        return version;
    }

    private static string Required(string? value, string message, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(message);
        var result = value.Trim();
        if (result.Length > maxLength) throw new ArgumentException($"Value cannot exceed {maxLength} characters.");
        return result;
    }

    private static string? Optional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var result = value.Trim();
        if (result.Length > maxLength) throw new ArgumentException($"Value cannot exceed {maxLength} characters.");
        return result;
    }

    [GeneratedRegex("^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$")]
    private static partial Regex KeyPattern();

    [GeneratedRegex("^(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)(?:-[0-9A-Za-z.-]+)?(?:\\+[0-9A-Za-z.-]+)?$")]
    private static partial Regex VersionPattern();
}

public sealed record CreateTemplatePackageRequest(string Key, string Name, string Category, string? Description);
public sealed record UpdateTemplatePackageRequest(string Name, string Category, string? Description);
public sealed record GenerateTemplateReleaseRequest(string Version, string? ChangeNotes);
public sealed record RejectTemplateReleaseRequest(string ReviewNotes);
public sealed record SaveTemplateDraftRequest(JsonElement Definition);
public sealed record FigmaImportRequest(string FileUrl, string AccessToken, string? NodeId = null);
