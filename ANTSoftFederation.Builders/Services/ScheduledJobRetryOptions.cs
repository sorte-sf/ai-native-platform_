namespace SorteFederation.Builders.Services;

/// <summary>Retry policy applied uniformly to Builder cron schedules.</summary>
public sealed class ScheduledJobRetryOptions
{
    public int Retries { get; set; } = 3;
    public int[] RetryIntervalsSeconds { get; set; } = [30, 120, 600];

    public (int Retries, int[] Intervals) GetValidatedPolicy()
    {
        var retries = Math.Clamp(Retries, 0, 10);
        var intervals = RetryIntervalsSeconds
            .Where(seconds => seconds > 0)
            .Take(10)
            .ToArray();

        if (retries > 0 && intervals.Length == 0)
            intervals = [30];

        return (retries, intervals);
    }
}
