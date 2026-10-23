using SorteFederation.Shared.Contracts;
using TickerQ.Utilities;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace SorteFederation.Builders.Endpoints;

public sealed class ScheduledJobsEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapScheduledJobsEndpoints(endpoints);

    public static IEndpointRouteBuilder MapScheduledJobsEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/applications/{applicationId:guid}/scheduled-jobs")
            .RequireAuthorization().WithTags("Scheduled Jobs");

        group.MapGet("", async (Guid applicationId, Guid tenantId, BuilderDbContext db, CancellationToken token) =>
        {
            if (!await db.Applications.AnyAsync(x => x.Id == applicationId && x.TenantId == tenantId, token)) return Results.NotFound();
            var items = await db.ScheduledJobs.AsNoTracking()
                .Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId)
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => new { x.Id, x.TenantId, x.ApplicationId, x.Key, x.Schedule, x.ScheduleType, x.RunAt, x.TimeZoneId, x.ActionType, x.ActionJson, x.Enabled, x.LastRunAt, x.NextRunAt, x.CreatedAt, x.UpdatedAt })
                .ToListAsync(token);
            return Results.Ok(items);
        });

        group.MapGet("/{jobId:guid}/latest-report", async (Guid applicationId, Guid jobId, Guid tenantId,
            BuilderDbContext db, ScheduledReportArtifactStore artifacts, CancellationToken token) =>
        {
            var job = await db.ScheduledJobs.AsNoTracking().SingleOrDefaultAsync(
                x => x.Id == jobId && x.ApplicationId == applicationId && x.TenantId == tenantId && x.ActionType == "report.generate", token);
            if (job is null) return Results.NotFound();
            var path = artifacts.GetLatestPath(jobId);
            return path is null
                ? Results.NotFound("No report export has been generated for this schedule yet.")
                : Results.File(path, "text/csv; charset=utf-8", $"{job.Key}-latest.csv");
        });

        group.MapGet("/templates", async (Guid applicationId, Guid tenantId, BuilderDbContext db, CancellationToken token) =>
        {
            if (!await db.Applications.AnyAsync(x => x.Id == applicationId && x.TenantId == tenantId, token)) return Results.NotFound();
            return Results.Ok(await db.Templates.AsNoTracking()
                .Where(x => x.IsBuiltIn && x.Category.StartsWith("Job Schedule:"))
                .OrderBy(x => x.Category).ThenBy(x => x.Name)
                .Select(x => new { x.Id, x.Key, x.Name, x.Description, Category = x.Category.Substring("Job Schedule: ".Length), x.ManifestJson })
                .ToListAsync(token));
        });

        group.MapPost("", async (Guid applicationId, CreateScheduledJobRequest request, BuilderDbContext db,
            ScheduledJobTimeZoneService timeZoneService, CancellationToken token) =>
        {
            if (request.ApplicationId != applicationId) return Results.BadRequest("Application id mismatch.");
            if (!await db.Applications.AnyAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token)) return Results.NotFound("Application not found.");
            if (string.IsNullOrWhiteSpace(request.Key) || request.Key.Length > 120) return Results.BadRequest("Enter a job name up to 120 characters.");
            if (await db.ScheduledJobs.AnyAsync(x => x.TenantId == request.TenantId && x.ApplicationId == applicationId && x.Key == request.Key.Trim(), token))
                return Results.Conflict("A scheduled job with this name already exists in the application.");
            var scheduleType = (request.ScheduleType ?? "recurring").Trim().ToLowerInvariant();
            if (scheduleType is not ("once" or "recurring")) return Results.BadRequest("Choose a one-time or recurring schedule.");
            if (scheduleType == "recurring" && (!IsSupportedCron(request.Schedule) || !ScheduledJobTimeZoneService.IsValidSchedule(request.Schedule))) return Results.BadRequest("Use a valid six-part cron schedule: second minute hour day month weekday.");
            if (!ScheduledJobTimeZoneService.IsValidTimeZone(request.TimeZoneId)) return Results.BadRequest("Choose a valid time zone.");
            DateTimeOffset? runAt = null;
            if (scheduleType == "once")
            {
                if (request.LocalRunAt is null) return Results.BadRequest("Choose the date and time for this one-time schedule.");
                try { runAt = ScheduledJobTimeZoneService.ConvertLocalTimeToUtc(request.LocalRunAt.Value, request.TimeZoneId); }
                catch (ArgumentException exception) { return Results.BadRequest(exception.Message); }
                if (runAt <= DateTimeOffset.UtcNow) return Results.BadRequest("Choose a future date and time.");
            }
            if (request.ActionType is not ("event.audit" or "notification.create" or "workflow.start" or "report.generate" or "email.send" or "http.request")) return Results.BadRequest("Choose a supported action type.");
            try
            {
                using var action = JsonDocument.Parse(request.ActionJson);
                if (action.RootElement.ValueKind != JsonValueKind.Object) return Results.BadRequest("Action details must be a JSON object.");
                if (!IsValidAction(request.ActionType, action.RootElement, out var actionError)) return Results.BadRequest(actionError);
            }
            catch (JsonException) { return Results.BadRequest("Action details must be valid JSON."); }

            var entity = new BuilderScheduledJob
            {
                TenantId = request.TenantId, ApplicationId = applicationId, Key = request.Key.Trim(),
                Schedule = scheduleType == "once" ? "once" : request.Schedule.Trim(), ScheduleType = scheduleType,
                RunAt = runAt, TimeZoneId = request.TimeZoneId, ActionType = request.ActionType, ActionJson = request.ActionJson,
                Enabled = true
            };
            db.ScheduledJobs.Add(entity);
            await db.SaveChangesAsync(token);

            try
            {
                await timeZoneService.EnsureScheduledAsync(entity, token);
            }
            catch (Exception) when (!token.IsCancellationRequested)
            {
                db.ScheduledJobs.Remove(entity);
                await db.SaveChangesAsync(token);
                return Results.Problem("The job could not be registered with the scheduler. No job was saved.", statusCode: 503);
            }
            return Results.Created($"/api/v1/applications/{applicationId}/scheduled-jobs/{entity.Id}", entity);
        });

        group.MapPatch("/{jobId:guid}/enabled", async (Guid applicationId, Guid jobId, SetScheduledJobEnabledRequest request,
            BuilderDbContext db, ScheduledJobTimeZoneService timeZoneService, CancellationToken token) =>
        {
            var job = await db.ScheduledJobs.SingleOrDefaultAsync(x => x.Id == jobId && x.ApplicationId == applicationId && x.TenantId == request.TenantId, token);
            if (job is null) return Results.NotFound();
            if (request.Enabled && string.Equals(job.ScheduleType, "once", StringComparison.OrdinalIgnoreCase) && job.RunAt <= DateTimeOffset.UtcNow)
                return Results.BadRequest("This one-time schedule has already passed. Create a new one-time schedule instead.");
            job.Enabled = request.Enabled;
            job.UpdatedAt = DateTimeOffset.UtcNow;
            job.Version++;
            await db.SaveChangesAsync(token);
            try { await timeZoneService.EnsureScheduledAsync(job, token); }
            catch (Exception) when (!token.IsCancellationRequested)
            {
                job.Enabled = !request.Enabled;
                job.Version++;
                await db.SaveChangesAsync(token);
                return Results.Problem("The scheduler could not be updated. The previous state was restored.", statusCode: 503);
            }
            return Results.Ok(new { job.Id, job.Enabled });
        });

        group.MapDelete("/{jobId:guid}", async (Guid applicationId, Guid jobId, Guid tenantId,
            BuilderDbContext db, ICronTickerManager<CronTickerEntity> tickerManager,
            ITimeTickerManager<TimeTickerEntity> timeTickerManager, CancellationToken token) =>
        {
            var job = await db.ScheduledJobs.SingleOrDefaultAsync(x => x.Id == jobId &&
                x.ApplicationId == applicationId && x.TenantId == tenantId, token);
            if (job is null) return Results.NotFound();
            try
            {
                if (job.NextTickerId is { } nextTickerId && await db.TimeTickers.AnyAsync(x => x.Id == nextTickerId, token))
                    await timeTickerManager.DeleteAsync(nextTickerId, token);
                if (await db.CronTickers.AnyAsync(x => x.Id == job.Id, token))
                    await tickerManager.DeleteAsync(job.Id, token);
            }
            catch (Exception) when (!token.IsCancellationRequested)
            {
                return Results.Problem("The scheduler could not be updated. The job was not deleted.", statusCode: 503);
            }
            db.ScheduledJobs.Remove(job);
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        });

        return endpoints;
    }

    private static bool IsSupportedCron(string expression)
    {
        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 6 && parts.All(x => x.Length <= 32 && x.All(c => char.IsAsciiLetterOrDigit(c) || c is '*' or '/' or ',' or '-' or '#'));
    }

    private static bool IsValidAction(string actionType, JsonElement action, out string error)
    {
        string? Read(string key) => action.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        bool ValidReportEmail()
        {
            if (!action.TryGetProperty("email", out var email)) return true;
            if (email.ValueKind != JsonValueKind.Object || !email.TryGetProperty("to", out var to) ||
                to.ValueKind != JsonValueKind.String || !MailAddress.TryCreate(to.GetString(), out _)) return false;
            foreach (var optional in new[] { "subject", "body" })
                if (email.TryGetProperty(optional, out var field) &&
                    (field.ValueKind != JsonValueKind.String || field.GetString()!.Length > (optional == "subject" ? 200 : 10_000))) return false;
            return true;
        }
        var valid = actionType switch
        {
            "notification.create" => !string.IsNullOrWhiteSpace(Read("recipientId")) && !string.IsNullOrWhiteSpace(Read("title")) && !string.IsNullOrWhiteSpace(Read("message")),
            "workflow.start" => Guid.TryParse(Read("workflowId"), out _) && Guid.TryParse(Read("recordId"), out _),
            "report.generate" => (Read("reportKey") is null || Read("reportKey")!.Length <= 120) && ValidReportEmail(),
            "email.send" => MailAddress.TryCreate(Read("to"), out _) && !string.IsNullOrWhiteSpace(Read("subject")) && !string.IsNullOrWhiteSpace(Read("body")),
            "http.request" => Uri.TryCreate(Read("url"), UriKind.Absolute, out var apiUri) && apiUri.Scheme == Uri.UriSchemeHttps &&
                string.IsNullOrEmpty(apiUri.UserInfo) && !IPAddress.TryParse(apiUri.Host, out _) &&
                Read("method") is { } method && new[] { "GET", "POST", "PUT", "PATCH", "DELETE" }.Contains(method, StringComparer.OrdinalIgnoreCase),
            _ => true
        };
        error = actionType switch
        {
            "notification.create" => "Notification actions need recipientId, title, and message.",
            "workflow.start" => "Workflow actions need valid workflowId and recordId values.",
            "report.generate" => "Report key must be 120 characters or fewer. Optional email delivery requires a valid recipient and a subject/body within the allowed length.",
            "email.send" => "Email actions need a valid to address, subject, and body.",
            "http.request" => "API actions need an HTTPS URL with a DNS hostname and a supported HTTP method.",
            _ => "Action details are invalid."
        };
        return valid;
    }

    internal static CronTickerEntity CreateTicker(BuilderScheduledJob job, ScheduledJobRetryOptions? retryOptions = null)
    {
        var message = new BuilderScheduledJobMessage(job.Id, job.TenantId, job.ApplicationId,
            job.Key, job.Schedule, job.ActionType, job.ActionJson, job.Enabled);
        var retryPolicy = (retryOptions ?? new ScheduledJobRetryOptions()).GetValidatedPolicy();
        return new CronTickerEntity
        {
            Id = job.Id,
            Function = "ExecuteBuilderScheduledJob",
            Description = $"{job.Key} · {job.ApplicationId}",
            Expression = job.Schedule,
            Request = TickerHelper.CreateTickerRequest(message),
            IsEnabled = job.Enabled,
            Retries = retryPolicy.Retries,
            RetryIntervals = retryPolicy.Intervals
        };
    }
}

public sealed record SetScheduledJobEnabledRequest(Guid TenantId, bool Enabled);
