using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SorteFederation.Shared.Realtime;

/// <summary>Gives each API an independent durable subscription and dispatches messages to its handlers.</summary>
public sealed class RealtimeEventInbox(
    IRealtimeEventBus eventBus,
    IServiceScopeFactory scopeFactory,
    ILogger<RealtimeEventInbox> logger,
    string subscriberName) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await eventBus.SubscribeAsync(subscriberName, async (message, token) =>
            {
                logger.LogDebug("Received realtime event {EventName} ({EventId}) for tenant {TenantId}.", message.EventName, message.EventId, message.TenantId);
                await using var scope = scopeFactory.CreateAsyncScope();
                foreach (var handler in scope.ServiceProvider.GetServices<IRealtimeEventHandler>())
                    await handler.HandleAsync(message, token);
            }, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // SubscribeAsync waits for the consumer lifetime; cancellation is expected on host shutdown.
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Realtime event subscription for {SubscriberName} stopped unexpectedly.", subscriberName);
            throw;
        }
    }
}
