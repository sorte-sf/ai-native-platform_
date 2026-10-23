using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SorteFederation.Builders.Hubs;

public class AntChatHub() : Hub, IDisposable
{
    private static readonly ConcurrentDictionary<string, List<string>> TenantConnections = new ConcurrentDictionary<string, List<string>>();

    private static int _activeConnections = 0;

    public string GetConnectionID()
    {
        return Context.ConnectionId;
    }

    public async Task AddToGroup(string readerGroup)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, readerGroup);
        Console.WriteLine($"User connected: {Context.ConnectionId}");
    }

    public async Task UserJoinTenant(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
        Interlocked.Increment(ref _activeConnections);
        await Clients.All.SendAsync("connectionCountUpdated", _activeConnections);
    }

    public async Task RemoveFromGroup(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AntSoft");

        Interlocked.Increment(ref _activeConnections);
        await Clients.All.SendAsync("connectionCountUpdated", _activeConnections);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AntSoft");

        // Update connection count
        Interlocked.Decrement(ref _activeConnections);
        await Clients.All.SendAsync("connectionCountUpdated", _activeConnections);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToTenant(string tenantId, string message)
    {
        if (TenantConnections.TryGetValue(tenantId, out var connectionIds))
        {
            foreach (var connectionId in connectionIds)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }
        }
    }

    public void Dispose()
    {
        // Cleanup resources if needed
    }

}

