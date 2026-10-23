using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SorteFederation.Shared.TemplateCatalog;

/// <summary>Durable broadcast events for immutable template catalog releases.</summary>
public sealed class RabbitMqTemplateCatalogEventBus(
    IConnection connection,
    ILogger<RabbitMqTemplateCatalogEventBus> logger) : ITemplateCatalogEventBus
{
    public const string ExchangeName = "antsoft.template.catalog.events";
    public const string ReleasePublishedRoute = "template.release.published";
    private const string BuildersQueueName = "antsoft.template.catalog.builders";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync(TemplateReleasePublishedEvent message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.TemplateId == Guid.Empty || message.ReleaseId == Guid.Empty ||
            string.IsNullOrWhiteSpace(message.TemplateKey) || string.IsNullOrWhiteSpace(message.Version) ||
            string.IsNullOrWhiteSpace(message.ManifestJson))
            throw new ArgumentException("A published template event must identify its template and release.", nameof(message));

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        // Declare the durable consumer queue before publishing. Exchanges do not
        // retain messages without a bound queue, and Aspire may start the
        // Designer Studio replay before Builder's hosted subscriber is ready.
        await channel.QueueDeclareAsync(BuildersQueueName, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(BuildersQueueName, ExchangeName, ReleasePublishedRoute, cancellationToken: cancellationToken);
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = message.ReleaseId.ToString("N")
        };
        var payload = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
        await channel.BasicPublishAsync(ExchangeName, ReleasePublishedRoute, mandatory: true,
            basicProperties: properties, body: payload, cancellationToken: cancellationToken);
    }

    public async Task SubscribeAsync(
        string subscriberName,
        Func<TemplateReleasePublishedEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberName);
        ArgumentNullException.ThrowIfNull(handler);
        if (subscriberName.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_'))
            throw new ArgumentException("Subscriber names may contain only letters, digits, hyphens, and underscores.", nameof(subscriberName));

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        var queue = $"antsoft.template.catalog.{subscriberName}";
        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queue, ExchangeName, ReleasePublishedRoute, cancellationToken: cancellationToken);
        await channel.BasicQosAsync(0, 1, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<TemplateReleasePublishedEvent>(delivery.Body.Span, JsonOptions)
                    ?? throw new JsonException("Template release event payload was empty.");
                await handler(message, cancellationToken);
                await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(exception, "Template catalog event processing failed; event {DeliveryTag} will be retried.", delivery.DeliveryTag);
                await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: true, cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(queue, autoAck: false, consumer, cancellationToken);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
