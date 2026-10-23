
namespace SorteFederation.Realtime.Services;

public sealed class RealtimeEventForwarder(
    IRealtimeEventBus eventBus,
    IHubContext<RealtimeHub> hub,
    ILogger<RealtimeEventForwarder> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await eventBus.SubscribeAsync("signalr-gateway", async (message, token) =>
            {
                if (!message.UserFacing) return;
                if (message.TenantId == Guid.Empty)
                {
                    logger.LogWarning("Ignoring user-facing realtime event {EventId} without a tenant.", message.EventId);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(message.UserId))
                    await hub.Clients.Group(RealtimeHub.TenantUserGroup(message.TenantId, message.UserId)).SendAsync("RealtimeEvent", message, token);
                else
                    await hub.Clients.Group(RealtimeHub.TenantGroup(message.TenantId)).SendAsync("RealtimeEvent", message, token);
            }, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Realtime event subscription stopped unexpectedly.");
            throw;
        }
    }
}
