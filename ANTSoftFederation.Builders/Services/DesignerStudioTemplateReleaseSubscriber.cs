using SorteFederation.Shared.TemplateCatalog;

namespace SorteFederation.Builders.Services;

/// <summary>Imports newly published Design Studio releases into Builder's local catalog.</summary>
public sealed class DesignerStudioTemplateReleaseSubscriber(
    ITemplateCatalogEventBus eventBus,
    IServiceScopeFactory scopeFactory,
    ILogger<DesignerStudioTemplateReleaseSubscriber> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await eventBus.SubscribeAsync("builders", async (message, token) =>
            {
                // Older queued notifications did not include the release manifest.
                // A Design Studio startup replay will emit the complete event.
                if (string.IsNullOrWhiteSpace(message.ManifestJson))
                {
                    logger.LogWarning("Skipping legacy Designer Studio notification for {TemplateKey} {Version}; awaiting the catalog replay.",
                        message.TemplateKey, message.Version);
                    return;
                }

                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    await scope.ServiceProvider.GetRequiredService<DesignerStudioCatalogSyncService>().ImportAsync(message, token);
                    logger.LogInformation("Imported Designer Studio template {TemplateKey} release {Version} ({ReleaseId}).",
                        message.TemplateKey, message.Version, message.ReleaseId);
                }
                catch (Exception exception) when (!token.IsCancellationRequested)
                {
                    if (exception is ArgumentException or InvalidOperationException)
                    {
                        // Invalid release payloads are permanent failures. ACK them so
                        // one malformed release cannot block every later catalog event.
                        logger.LogError(exception, "Rejected invalid Designer Studio template {TemplateKey} release {Version}; it will not be retried.",
                            message.TemplateKey, message.Version);
                        return;
                    }

                    logger.LogError(exception, "Could not import Designer Studio template {TemplateKey} release {Version}; RabbitMQ will retry the event.",
                        message.TemplateKey, message.Version);
                    throw;
                }
            }, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected when the Builder host is stopping.
        }
    }
}
