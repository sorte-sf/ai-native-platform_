using SorteFederation.Builders.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.Builders.Services;

public sealed class WorkflowExecutionWorker(
    IServiceScopeFactory scopes,
    ILogger<WorkflowExecutionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await using var scope = scopes.CreateAsyncScope();
                    var metadata = scope.ServiceProvider.GetRequiredService<BuilderDbContext>();
                    foreach (var tenantId in await metadata.Workflows.Where(x => x.Status == DefinitionStatus.Published).Select(x => x.TenantId).Distinct().ToListAsync(stoppingToken))
                        await ProcessTenantAsync(tenantId, scope.ServiceProvider.GetRequiredService<TenantRuntimeContextFactory>(), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
                catch (Exception ex) { logger.LogError(ex, "Workflow execution cycle failed."); }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task ProcessTenantAsync(Guid tenantId, TenantRuntimeContextFactory factory, CancellationToken token)
    {
        await using var db = await factory.CreateForWorkerAsync(tenantId, token);
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(token);
            var now = DateTimeOffset.UtcNow;
            var tasks = await db.WorkflowTasks.FromSqlInterpolated($"""
                SELECT * FROM builders_runtime."WorkflowTasks"
                WHERE "TenantId" = {tenantId} AND NOT "Completed" AND "Activated"
                  AND ("DueAt" <= {now} OR "RetryAt" <= {now} OR "EscalateAt" <= {now})
                ORDER BY COALESCE("DueAt", "RetryAt", "EscalateAt")
                LIMIT 100 FOR UPDATE SKIP LOCKED
                """)
                .Where(x => x.TenantId == tenantId && !x.Completed && x.Activated && x.WorkflowInstance.Status == WorkflowInstanceStatus.Running)
                .ToListAsync(token);
            var instanceIds = tasks.Select(x => x.WorkflowInstanceId).Distinct().ToArray();
            var instances = await db.WorkflowInstances.Where(x => instanceIds.Contains(x.Id) && x.Status == WorkflowInstanceStatus.Running)
                .ToDictionaryAsync(x => x.Id, token);
            foreach (var task in tasks)
            {
                if (!instances.TryGetValue(task.WorkflowInstanceId, out var instance)) continue;
                if (task.EscalateAt <= now)
                {
                    if (!string.IsNullOrWhiteSpace(task.EscalateTo)) task.AssignedTo = task.EscalateTo;
                    task.EscalateAt = null;
                }
                if (task.RetryAt <= now)
                {
                    task.RetryCount++;
                    task.RetryAt = task.RetryCount < task.MaxRetries ? now.AddSeconds(Math.Min(300, 15 * task.RetryCount)) : null;
                    if (task.RetryCount >= task.MaxRetries)
                    {
                        instance.Status = WorkflowInstanceStatus.Failed;
                        instance.LastError = task.LastError ?? "Workflow task exceeded its retry limit.";
                        instance.Version++;
                    }
                }
                if (task.DueAt <= now) task.DueAt = null;
                task.Version++;
                task.UpdatedAt = now;
            }
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        });
    }
}
