using SorteFederation.DesignerStudio.Data;
using SorteFederation.DesignerStudio.Data.Entities;
using SorteFederation.Shared.TemplateCatalog;

namespace SorteFederation.DesignerStudio.Services;

/// <summary>Replays published releases so Builder can recover after downtime or first startup.</summary>
public sealed class PublishedTemplateCatalogReplayService(
    IServiceScopeFactory scopeFactory,
    ILogger<PublishedTemplateCatalogReplayService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<DesignerStudioDbContext>();
                var eventBus = scope.ServiceProvider.GetRequiredService<ITemplateCatalogEventBus>();
                var releases = await db.TemplatePackageReleases.AsNoTracking()
                    .Where(x => x.Status == TemplateReleaseStatus.Published)
                    .Include(x => x.TemplatePackage)
                    .ToListAsync(stoppingToken);

                foreach (var release in releases.OrderBy(x => x.PublishedAt).ThenBy(x => x.TemplatePackage.Key))
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
                        release.ManifestJson), stoppingToken);
                }

                logger.LogInformation("Replayed {ReleaseCount} published Designer Studio releases to the template catalog.", releases.Count);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Could not replay the published template catalog; retrying shortly.");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
