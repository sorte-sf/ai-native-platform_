//using SorteFederation.Shared.Contracts;
//using NCrontab;
//using Microsoft.Extensions.Options;
//using System.Security.Cryptography;
//using System.Text;
//using TickerQ.Utilities;
//using TickerQ.Utilities.Entities;
//using TickerQ.Utilities.Interfaces.Managers;
//using TickerStatus = global::TickerQ.Utilities.Enums.TickerStatus;

//namespace SorteFederation.Builders.Services;

///// <summary>Schedules each job's next wall-clock cron occurrence as a UTC TickerQ one-shot.</summary>
//public sealed class ScheduledJobTimeZoneService(
//    BuilderDbContext db,
//    ITimeTickerManager<TimeTickerEntity> tickerManager,
//    IOptions<ScheduledJobRetryOptions> retryOptions)
//{
//    public static bool IsValidTimeZone(string? timeZoneId)
//    {
//        if (string.IsNullOrWhiteSpace(timeZoneId) || timeZoneId.Length > 100) return false;
//        try { _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); return true; }
//        catch (TimeZoneNotFoundException) { return false; }
//        catch (InvalidTimeZoneException) { return false; }
//    }

//    public static bool IsValidSchedule(string expression)
//    {
//        try { _ = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions { IncludingSeconds = true }); return true; }
//        catch (CrontabException) { return false; }
//        catch (ArgumentException) { return false; }
//    }

//    public static DateTimeOffset GetNextOccurrenceUtc(string expression, string timeZoneId, DateTimeOffset now)
//    {
//        if (!IsValidTimeZone(timeZoneId)) throw new ArgumentException("Choose a valid time zone.", nameof(timeZoneId));
//        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
//        var cron = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
//        var localNow = TimeZoneInfo.ConvertTime(now, zone).DateTime;
//        var candidate = DateTime.SpecifyKind(localNow.AddSeconds(1), DateTimeKind.Unspecified);
//        var limit = DateTime.SpecifyKind(localNow.AddYears(5), DateTimeKind.Unspecified);

//        while (candidate < limit)
//        {
//            candidate = cron.GetNextOccurrence(candidate);
//            if (zone.IsInvalidTime(candidate)) continue;

//            var offsets = zone.IsAmbiguousTime(candidate) ? zone.GetAmbiguousTimeOffsets(candidate) : [zone.GetUtcOffset(candidate)];
//            var instants = offsets.Select(offset => new DateTimeOffset(candidate, offset).ToUniversalTime())
//                .Where(instant => instant > now).OrderBy(instant => instant).ToArray();
//            if (instants.Length > 0) return instants[0];
//        }

//        throw new InvalidOperationException("No valid cron occurrence was found in the next five years.");
//    }

//    public static DateTimeOffset ConvertLocalTimeToUtc(DateTime localDateTime, string timeZoneId)
//    {
//        if (!IsValidTimeZone(timeZoneId)) throw new ArgumentException("Choose a valid time zone.", nameof(timeZoneId));
//        var local = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
//        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
//        if (zone.IsInvalidTime(local)) throw new ArgumentException("That local time does not exist because the clocks move forward. Choose another time.", nameof(localDateTime));
//        var offsets = zone.IsAmbiguousTime(local) ? zone.GetAmbiguousTimeOffsets(local) : [zone.GetUtcOffset(local)];
//        return offsets.Select(offset => new DateTimeOffset(local, offset).ToUniversalTime()).Min();
//    }

//    public async Task EnsureScheduledAsync(BuilderScheduledJob job, CancellationToken cancellationToken)
//    {
//        if (!job.Enabled)
//        {
//            if (job.NextTickerId is { } disabledId)
//            {
//                await tickerManager.DeleteAsync(disabledId, cancellationToken);
//                job.NextTickerId = null;
//                job.NextRunAt = null;
//                job.UpdatedAt = DateTimeOffset.UtcNow;
//                job.Version++;
//                await db.SaveChangesAsync(cancellationToken);
//            }
//            return;
//        }

//        if (job.NextTickerId is { } currentId)
//        {
//            var current = await db.TimeTickers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == currentId, cancellationToken);
//            if (current is not null && current.Status is TickerStatus.Idle or TickerStatus.Queued or TickerStatus.InProgress)
//                return;
//            if (current is not null && string.Equals(job.ScheduleType, "once", StringComparison.OrdinalIgnoreCase))
//            {
//                job.Enabled = false;
//                job.NextRunAt = null;
//                job.UpdatedAt = DateTimeOffset.UtcNow;
//                job.Version++;
//                await db.SaveChangesAsync(cancellationToken);
//                return;
//            }
//            if (current is not null) await tickerManager.DeleteAsync(currentId, cancellationToken);
//        }

//        var nextRun = string.Equals(job.ScheduleType, "once", StringComparison.OrdinalIgnoreCase)
//            ? job.RunAt ?? throw new InvalidOperationException("A one-time schedule needs a run date and time.")
//            : GetNextOccurrenceUtc(job.Schedule, job.TimeZoneId, DateTimeOffset.UtcNow);
//        if (nextRun <= DateTimeOffset.UtcNow)
//        {
//            job.Enabled = false;
//            job.NextRunAt = null;
//            job.UpdatedAt = DateTimeOffset.UtcNow;
//            job.Version++;
//            await db.SaveChangesAsync(cancellationToken);
//            return;
//        }
//        var tickerId = new Guid(SHA256.HashData(Encoding.UTF8.GetBytes($"{job.Id:N}:{nextRun.UtcTicks}"))[..16]);
//        var retry = retryOptions.Value.GetValidatedPolicy();
//        var request = new BuilderScheduledJobMessage(job.Id, job.TenantId, job.ApplicationId,
//            job.Key, job.Schedule, job.ActionType, job.ActionJson, job.Enabled);
//        var ticker = new TimeTickerEntity
//        {
//            Id = tickerId,
//            Function = "ExecuteBuilderScheduledJob",
//            Description = $"{job.Key} · {job.TimeZoneId}",
//            Request = global::TickerQ.Utilities.TickerHelper.CreateTickerRequest(request),
//            ExecutionTime = nextRun.UtcDateTime,
//            Retries = retry.Retries,
//            RetryIntervals = retry.Intervals
//        };
//        await tickerManager.AddAsync(ticker, cancellationToken);
//        job.NextTickerId = tickerId;
//        job.NextRunAt = nextRun;
//        job.UpdatedAt = DateTimeOffset.UtcNow;
//        job.Version++;
//        try { await db.SaveChangesAsync(cancellationToken); }
//        catch
//        {
//            await tickerManager.DeleteAsync(tickerId, CancellationToken.None);
//            throw;
//        }
//    }
//}
