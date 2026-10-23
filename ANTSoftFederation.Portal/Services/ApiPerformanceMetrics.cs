using System.Collections.Concurrent;
using System.Diagnostics;


namespace SorteFederation.ANTApplications.Services;

public sealed class ApiPerformanceMetrics
{
    private long _requests;
    private long _errors;
    private long _elapsedMs;

    public void Record(long elapsedMs, bool error)
    {
        Interlocked.Increment(ref _requests);
        Interlocked.Add(ref _elapsedMs, elapsedMs);
        if (error) Interlocked.Increment(ref _errors);
    }

    public ApiPerformanceSnapshot Snapshot()
    {
        var requests = Interlocked.Read(ref _requests);
        return new ApiPerformanceSnapshot(requests, Interlocked.Read(ref _errors), requests == 0 ? 0 : (double)Interlocked.Read(ref _elapsedMs) / requests);
    }
}

public sealed record ApiPerformanceSnapshot(long Requests, long Errors, double AverageLatencyMs);

public sealed class ApiPerformanceMetricsMiddleware(RequestDelegate next, ApiPerformanceMetrics metrics)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            await next(context);
            metrics.Record(timer.ElapsedMilliseconds, context.Response.StatusCode >= 500);
        }
        catch
        {
            metrics.Record(timer.ElapsedMilliseconds, true);
            throw;
        }
    }
}
