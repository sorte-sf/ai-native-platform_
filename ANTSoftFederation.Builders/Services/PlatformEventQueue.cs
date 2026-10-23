using SorteFederation.Builders.Data.Enums;
using System.Threading.Channels;

namespace SorteFederation.Builders.Services;

public sealed record PlatformEvent(Guid Id, Guid TenantId, string Type, string PayloadJson, DateTimeOffset CreatedAt);

public sealed class PlatformEventQueue
{
    private readonly Channel<PlatformEvent> _events = Channel.CreateUnbounded<PlatformEvent>();
    public ValueTask Enqueue(PlatformEvent value, CancellationToken token) => _events.Writer.WriteAsync(value, token);
    public IAsyncEnumerable<PlatformEvent> ReadAll(CancellationToken token) => _events.Reader.ReadAllAsync(token);
}

public sealed class PlatformEventWorker(PlatformEventQueue queue, IServiceScopeFactory scopes, ILogger<PlatformEventWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RequeuePendingEvents(stoppingToken);
            await foreach (var value in queue.ReadAll(stoppingToken))
            {
                try
                {
                    logger.LogInformation("Processing platform event {Type} for tenant {TenantId}", value.Type, value.TenantId);
                    await using var scope = scopes.CreateAsyncScope();
                    var db = scope.ServiceProvider.GetRequiredService<BuilderDbContext>();
                    var persisted = await db.PlatformEvents.SingleOrDefaultAsync(x => x.Id == value.Id && x.TenantId == value.TenantId, stoppingToken);
                    if (persisted is not null)
                    {
                        await scope.ServiceProvider.GetRequiredService<PlatformActionDispatcher>().DispatchAsync(value, stoppingToken);
                        persisted.Status = PlatformEventStatus.Processed;
                        persisted.ProcessedAt = DateTimeOffset.UtcNow;
                        persisted.Attempts++;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogError(ex, "Platform event failed.");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task RequeuePendingEvents(CancellationToken token)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BuilderDbContext>();
        var pending = await db.PlatformEvents.AsNoTracking()
            .Where(x => x.Status == PlatformEventStatus.Pending || x.Status == PlatformEventStatus.Queued)
            .OrderBy(x => x.CreatedAt).Take(1000).ToListAsync(token);
        foreach (var value in pending)
        {
            await queue.Enqueue(new PlatformEvent(value.Id, value.TenantId, value.Type, value.PayloadJson, value.CreatedAt), token);
            logger.LogInformation("Requeued platform event {EventId} for tenant {TenantId}", value.Id, value.TenantId);
        }
    }
}
