using System.Text.Json;

namespace SorteFederation.Shared.Realtime;

/// <summary>A cross-service event. User-facing events are forwarded to connected clients in the same tenant.</summary>
public sealed record RealtimeEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; init; }
    public string EventName { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string? UserId { get; init; }
    public bool UserFacing { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public JsonElement? Data { get; init; }
}
