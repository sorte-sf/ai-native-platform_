using System.Text.Json;
using SorteFederation.Shared.TemplateCatalog;

namespace SorteFederation.Builders.Services;

/// <summary>
/// Imports a published Designer Studio release event into Builder's immutable
/// release store so application creation and updates remain version-pinned.
/// </summary>
public sealed class DesignerStudioCatalogSyncService(
    BuilderDbContext db)
{
    private const string ImportedKeyPrefix = "designer-studio:";
    private static readonly SemaphoreSlim SyncLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task ImportAsync(TemplateReleasePublishedEvent item, CancellationToken token)
    {
        await SyncLock.WaitAsync(token);
        try
        {
            if (item.TemplateId == Guid.Empty || item.ReleaseId == Guid.Empty ||
                string.IsNullOrWhiteSpace(item.TemplateKey) || string.IsNullOrWhiteSpace(item.TemplateName) ||
                string.IsNullOrWhiteSpace(item.Version) || string.IsNullOrWhiteSpace(item.ManifestJson))
                throw new InvalidOperationException("A published Designer Studio event is missing required template release data.");

            var normalizedManifestJson = TemplateManifestService.NormalizeImportedNavigationKeys(item.ManifestJson);
            var manifest = TemplateManifestService.Parse(normalizedManifestJson);
            var themeJson = BuildThemeJson(manifest);
            var key = ImportedKeyPrefix + item.TemplateKey;
            var template = await db.Templates.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Key == key, token);
            var isNewTemplate = template is null;
            template ??= new BuilderTemplate
            {
                Key = key,
                Name = item.TemplateName,
                HtmlContent = string.Empty,
                ScssContent = string.Empty,
                IsBuiltIn = true
            };
            var release = await db.TemplateReleases.IgnoreQueryFilters().SingleOrDefaultAsync(
                x => x.TemplateId == template.Id && x.ReleaseVersion == item.Version, token);
            // Builder's local mirror may already contain a compatibility-normalized
            // copy of this immutable Studio release. Keep existing release snapshots
            // untouched on redelivery; refresh the catalog's current-template view below.
            if (release is null)
            {
                var newRelease = new BuilderTemplateRelease
                {
                    TemplateId = template.Id,
                    ReleaseVersion = item.Version,
                    ManifestVersion = manifest.Version,
                    ManifestJson = normalizedManifestJson,
                    ThemeJson = themeJson,
                    HtmlContent = string.Empty,
                    ScssContent = string.Empty,
                    Description = item.Description,
                    ReleasedAt = item.PublishedAt
                };
                if (isNewTemplate)
                {
                    template.Releases.Add(newRelease);
                    db.Templates.Add(template);
                }
                else
                {
                    db.TemplateReleases.Add(newRelease);
                }
            }

            template.Name = item.TemplateName;
            template.Description = item.Description;
            template.Category = string.IsNullOrWhiteSpace(item.Category) ? "General" : item.Category[..Math.Min(item.Category.Length, 64)];
            template.BlueprintVersion = item.Version;
            template.ManifestVersion = manifest.Version;
            template.ManifestJson = normalizedManifestJson;
            template.HtmlContent = string.Empty;
            template.ScssContent = string.Empty;
            template.PreviewImageUrl = null;
            template.BaseTemplate = null;
            template.ThemeJson = themeJson;
            template.IsBuiltIn = true;
            template.RecordState = RecordState.Active;
            template.UpdatedAt = DateTimeOffset.UtcNow;
            if (!isNewTemplate) db.Templates.Update(template);
            await db.SaveChangesAsync(token);
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private static string BuildThemeJson(TemplateManifest manifest)
    {
        var design = manifest.Theme;
        var shell = manifest.Shell;
        var fontSize = design?.FontSize ?? 16;
        var theme = new Dictionary<string, object?>
        {
            ["primaryColor"] = design?.AccentColor ?? "#5e4fa2",
            ["backgroundColor"] = design?.PageBackground ?? "#ffffff",
            ["surfaceColor"] = shell?.SidebarBackground ?? "#ffffff",
            ["textColor"] = design?.TextColor ?? "#253044",
            ["logoUrl"] = shell?.LogoUrl ?? "",
            ["fontScale"] = Math.Clamp(fontSize / 16, 0.8, 1.4),
            ["fontFamily"] = design?.FontFamily ?? "Inter, Arial, sans-serif",
            ["fontSize"] = fontSize
        };
        if (shell is not null) theme["shell"] = shell;
        return TemplateThemeService.Validate(JsonSerializer.Serialize(theme, JsonOptions));
    }
}
