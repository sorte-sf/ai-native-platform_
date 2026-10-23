namespace SorteFederation.Shared.TemplateCatalog;

public interface ITemplateCatalogEventBus
{
    Task PublishAsync(TemplateReleasePublishedEvent message, CancellationToken cancellationToken = default);

    Task SubscribeAsync(
        string subscriberName,
        Func<TemplateReleasePublishedEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);
}
