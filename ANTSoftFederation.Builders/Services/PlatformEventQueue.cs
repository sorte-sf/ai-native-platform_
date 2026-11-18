using SorteFederation.Builders.Data.Enums;
using System.Threading.Channels;

namespace SorteFederation.Builders.Services;

public sealed record PlatformEvent(Guid Id, Guid TenantId, string Type, string PayloadJson, DateTimeOffset CreatedAt);

public sealed class PlatformEventQueue
{
    private readonly Channel<PlatformEvent> _events = Channel.CreateBounded<PlatformEvent>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false
    });
    public ValueTask Enqueue(PlatformEvent value, CancellationToken token) => _events.Writer.WriteAsync(value, token);
    public IAsyncEnumerable<PlatformEvent> ReadAll(CancellationToken token) => _events.Reader.ReadAllAsync(token);
}

public sealed class PlatformEventWorker(PlatformEventQueue queue, IServiceScopeFactory scopes, ILogger<PlatformEventWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RecoverEvents(stoppingToken);
            await Task.WhenAll(ConsumeEvents(stoppingToken), RecoverLoop(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task ConsumeEvents(CancellationToken stoppingToken)
    {
        await foreach (var value in queue.ReadAll(stoppingToken))
        {
            try
            {
                await ProcessEvent(value, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
            catch (Exception ex) { logger.LogError(ex, "Unexpected failure processing platform event {EventId}.", value.Id); }
        }
    }

    private async Task ProcessEvent(PlatformEvent value, CancellationToken token)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BuilderDbContext>();
        var now = DateTimeOffset.UtcNow;
        var claimed = await db.PlatformEvents
            .Where(x => x.Id == value.Id && x.TenantId == value.TenantId &&
                (x.Status == PlatformEventStatus.Pending || x.Status == PlatformEventStatus.Queued))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.Status, PlatformEventStatus.Processing)
                .SetProperty(x => x.Attempts, x => x.Attempts + 1)
                .SetProperty(x => x.UpdatedAt, now), token);
        if (claimed == 0) return;

        db.ChangeTracker.Clear();
        var persisted = await db.PlatformEvents.SingleOrDefaultAsync(x => x.Id == value.Id && x.TenantId == value.TenantId, token);
        if (persisted is null) return;
        try
        {
            logger.LogInformation("Processing platform event {Type} for tenant {TenantId}", value.Type, value.TenantId);
            await scope.ServiceProvider.GetRequiredService<PlatformActionDispatcher>().DispatchAsync(value, token);
            persisted.Status = PlatformEventStatus.Processed;
            persisted.ProcessedAt = DateTimeOffset.UtcNow;
            persisted.Error = null;
            persisted.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            db.ChangeTracker.Clear();
            persisted = await db.PlatformEvents.SingleOrDefaultAsync(x => x.Id == value.Id && x.TenantId == value.TenantId, token);
            if (persisted is null) return;
            persisted.Error = exception.Message.Length <= 4000 ? exception.Message : exception.Message[..4000];
            persisted.Status = persisted.Attempts >= 8 ? PlatformEventStatus.Failed : PlatformEventStatus.Pending;
            persisted.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);

            if (persisted.Status == PlatformEventStatus.Pending)
            {
                var delaySeconds = Math.Min(300, 2 << Math.Min(persisted.Attempts - 1, 7));
                _ = RetryLater(value, TimeSpan.FromSeconds(delaySeconds), token);
            }
            else logger.LogError(exception, "Platform event {EventId} exhausted its retry limit and was moved to Failed.", value.Id);
        }
    }

    private async Task RecoverLoop(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(token)) await RecoverEvents(token);
    }

    private async Task RetryLater(PlatformEvent value, TimeSpan delay, CancellationToken token)
    {
        try
        {
            await Task.Delay(delay, token);
            await queue.Enqueue(value, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception exception) { logger.LogError(exception, "Could not reschedule platform event {EventId}; persistent recovery will retry it.", value.Id); }
    }

    private async Task RecoverEvents(CancellationToken token)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BuilderDbContext>();
        var staleBefore = DateTimeOffset.UtcNow.AddMinutes(-10);
        await db.PlatformEvents
            .Where(x => x.Status == PlatformEventStatus.Processing && x.UpdatedAt < staleBefore)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.Status, PlatformEventStatus.Pending)
                .SetProperty(x => x.Error, "Recovered after the event worker stopped before acknowledging completion.")
                .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow), token);

        var retryDueBefore = DateTimeOffset.UtcNow.AddMinutes(-5);
        var pending = await db.PlatformEvents.AsNoTracking()
            .Where(x => x.Status == PlatformEventStatus.Queued ||
                (x.Status == PlatformEventStatus.Pending && x.UpdatedAt < retryDueBefore))
            .OrderBy(x => x.CreatedAt).Take(100).ToListAsync(token);
        foreach (var value in pending)
            await queue.Enqueue(new PlatformEvent(value.Id, value.TenantId, value.Type, value.PayloadJson, value.CreatedAt), token);
    }
}
