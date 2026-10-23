namespace SorteFederation.Shared.Realtime;

/// <summary>Implement in a service to react to events published by any other service.</summary>
public interface IRealtimeEventHandler
{
    Task HandleAsync(RealtimeEvent message, CancellationToken cancellationToken);
}
