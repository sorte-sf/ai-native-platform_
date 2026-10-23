
namespace SorteFederation.ANTApplications.Services;

public sealed class TcmbExchangeRateBackgroundService(TcmbExchangeRateImporter importer, ILogger<TcmbExchangeRateBackgroundService> logger) : BackgroundService
{
    private static readonly TimeOnly[] RunTimes = [new(10, 0), new(14, 0)];
    private static readonly TimeZoneInfo Istanbul = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Istanbul);
                var next = RunTimes.Select(time => new DateTimeOffset(now.Date + time.ToTimeSpan(), now.Offset)).FirstOrDefault(value => value > now);
                if (next == default) next = new DateTimeOffset(now.Date.AddDays(1) + RunTimes[0].ToTimeSpan(), now.Offset);

                await Task.Delay(next - now, stoppingToken);
                try
                {
                    await importer.ImportAsync(DateOnly.FromDateTime(next.DateTime), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "TCMB exchange-rate import failed for {Date}", next.Date);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected when the host is shutting down.
        }
    }
}
