using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SorteFederation.Shared.Realtime;

/// <summary>
/// Durable topic-based event bus shared by the APIs. Each named subscriber gets its own queue,
/// so every participating service receives a copy without depending on a live SignalR connection.
/// </summary>
public sealed class RabbitMqRealtimeEventBus(IConnection connection, ILogger<RabbitMqRealtimeEventBus> logger) : IRealtimeEventBus
{
    public const string ExchangeName = "antsoft.realtime.events";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync(RealtimeEvent message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.TenantId == Guid.Empty)
            throw new ArgumentException("Realtime events require a tenant ID.", nameof(message));
        if (string.IsNullOrWhiteSpace(message.EventName) || message.EventName.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_'))
            throw new ArgumentException("Event names may contain only letters, digits, hyphens, and underscores.", nameof(message));

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        var route = $"tenant.{message.TenantId:N}.{message.EventName.ToLowerInvariant()}";
        var properties = new BasicProperties { Persistent = true, ContentType = "application/json", MessageId = message.EventId.ToString() };
        var payload = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
        await channel.BasicPublishAsync(ExchangeName, route, mandatory: true, basicProperties: properties, body: payload, cancellationToken: cancellationToken);
    }

    public async Task SubscribeAsync(
        string subscriberName,
        Func<RealtimeEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberName);
        ArgumentNullException.ThrowIfNull(handler);
        if (subscriberName.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_'))
            throw new ArgumentException("Subscriber names may contain only letters, digits, hyphens, and underscores.", nameof(subscriberName));

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        var queue = $"antsoft.realtime.{subscriberName}";
        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queue, ExchangeName, "tenant.#", cancellationToken: cancellationToken);
        await channel.BasicQosAsync(0, 16, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<RealtimeEvent>(delivery.Body.Span, JsonOptions)
                    ?? throw new JsonException("Realtime event payload was empty.");
                await handler(message, cancellationToken);
                await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(exception, "Realtime event handler failed for event {DeliveryTag}; the message will be retried.", delivery.DeliveryTag);
                await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: true, cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(queue, autoAck: false, consumer, cancellationToken);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
