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
        var now = DateTimeOffset.UtcNow;
        var tasks = await db.WorkflowTasks.Include(x => x.WorkflowInstance)
            .Where(x => x.TenantId == tenantId && !x.Completed && x.Activated &&
                x.WorkflowInstance.Status == WorkflowInstanceStatus.Running &&
                ((x.DueAt != null && x.DueAt <= now) || (x.RetryAt != null && x.RetryAt <= now) || (x.EscalateAt != null && x.EscalateAt <= now)))
            .OrderBy(x => x.DueAt ?? x.RetryAt ?? x.EscalateAt).Take(100).ToListAsync(token);
        foreach (var task in tasks)
        {
            if (task.EscalateAt <= now && !string.IsNullOrWhiteSpace(task.EscalateTo))
            {
                task.AssignedTo = task.EscalateTo;
                task.EscalateAt = null;
            }
            if (task.RetryAt <= now)
            {
                task.RetryCount++;
                task.RetryAt = task.RetryCount < task.MaxRetries ? now.AddSeconds(Math.Min(300, 15 * task.RetryCount)) : null;
                if (task.RetryCount >= task.MaxRetries) task.WorkflowInstance.Status = WorkflowInstanceStatus.Failed;
            }
            if (task.DueAt <= now) task.DueAt = null;
            task.UpdatedAt = now;
        }
        await db.SaveChangesAsync(token);
    }
}
