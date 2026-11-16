//using TickerQ.Utilities.Entities;
//using TickerQ.Utilities.Interfaces.Managers;
//using Microsoft.Extensions.Options;

//namespace SorteFederation.Builders.Services;

///// <summary>Restores persisted Builder schedules into TickerQ after deploys and database migrations.</summary>
//public sealed class ScheduledJobTickerReconciler(
//    IServiceScopeFactory scopeFactory,
//    ILogger<ScheduledJobTickerReconciler> logger) : BackgroundService
//{
//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
//        do
//        {
//            try
//            {
//                await using var scope = scopeFactory.CreateAsyncScope();
//                var db = scope.ServiceProvider.GetRequiredService<BuilderDbContext>();
//                var cronManager = scope.ServiceProvider.GetRequiredService<ICronTickerManager<CronTickerEntity>>();
//                var jobs = await db.ScheduledJobs.ToListAsync(stoppingToken);
//                var timeZoneService = scope.ServiceProvider.GetRequiredService<ScheduledJobTimeZoneService>();

//                foreach (var job in jobs)
//                {
//                    stoppingToken.ThrowIfCancellationRequested();
//                    // Remove legacy host-time-zone cron entries created before per-job zones were supported.
//                    if (await db.CronTickers.AnyAsync(x => x.Id == job.Id, stoppingToken))
//                        await cronManager.DeleteAsync(job.Id, stoppingToken);
//                    await timeZoneService.EnsureScheduledAsync(job, stoppingToken);
//                }
//                if (jobs.Count > 0)
//                    logger.LogInformation("Reconciled {JobCount} Builder scheduled jobs with TickerQ using their configured time zones.", jobs.Count);
//            }
//            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
//            catch (Exception exception)
//            {
//                logger.LogError(exception, "Could not reconcile Builder scheduled jobs with TickerQ; retrying on the next interval.");
//            }
//        } while (await timer.WaitForNextTickAsync(stoppingToken));
//    }
//}
