namespace SorteFederation.Shared.Realtime;

public interface IRealtimeEventBus
{
    Task PublishAsync(RealtimeEvent message, CancellationToken cancellationToken = default);

    Task SubscribeAsync(
        string subscriberName,
        Func<RealtimeEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);
}
