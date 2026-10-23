using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using SorteFederation.Shared.Contracts;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SorteFederation.Builders.Services;

public sealed record ScheduledJobActionContext(
    Guid JobId,
    Guid TenantId,
    Guid ApplicationId,
    string JobName,
    string ActionType,
    JsonElement Payload,
    DateTimeOffset TriggeredAtUtc);

public interface IScheduledJobActionHandler
{
    string ActionType { get; }
    Task ExecuteAsync(ScheduledJobActionContext context, CancellationToken cancellationToken);
}

public interface IScheduledJobActionDispatcher
{
    Task DispatchAsync(BuilderScheduledJobMessage job, CancellationToken cancellationToken = default);
}

public sealed class ScheduledJobActionDispatcher(
    IServiceScopeFactory scopes,
    ILogger<ScheduledJobActionDispatcher> logger) : IScheduledJobActionDispatcher
{
    public async Task DispatchAsync(BuilderScheduledJobMessage job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (job.JobId == Guid.Empty || job.TenantId == Guid.Empty || job.ApplicationId == Guid.Empty)
            throw new InvalidOperationException("A scheduled action requires job, tenant, and application IDs.");

        using var document = JsonDocument.Parse(job.ActionJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Scheduled action payload must be a JSON object.");

        var context = new ScheduledJobActionContext(job.JobId, job.TenantId, job.ApplicationId,
            job.Key, job.ActionType, document.RootElement.Clone(), DateTimeOffset.UtcNow);

        await using var scope = scopes.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<BuilderDbContext>();
        var persistedJob = await db.ScheduledJobs.SingleOrDefaultAsync(
            x => x.Id == job.JobId && x.TenantId == job.TenantId && x.ApplicationId == job.ApplicationId,
            cancellationToken);
        if (persistedJob is null || !persistedJob.Enabled)
        {
            logger.LogInformation("Skipping scheduled action for missing or disabled job {JobId}.", job.JobId);
            return;
        }

        var handlers = scope.ServiceProvider.GetServices<IScheduledJobActionHandler>().ToArray();

        var matches = handlers.Where(x => string.Equals(x.ActionType, job.ActionType, StringComparison.OrdinalIgnoreCase)).ToArray();

        if (matches.Length != 1)
            throw new InvalidOperationException(matches.Length == 0
                ? $"No scheduled action handler is registered for '{job.ActionType}'."
                : $"Multiple scheduled action handlers are registered for '{job.ActionType}'.");

        logger.LogInformation("Executing scheduled action {ActionType} for job {JobId} in tenant {TenantId}.",
            job.ActionType, job.JobId, job.TenantId);
        await matches[0].ExecuteAsync(context, cancellationToken);
        persistedJob.LastRunAt = context.TriggeredAtUtc;
        persistedJob.UpdatedAt = DateTimeOffset.UtcNow;
        persistedJob.Version++;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Scheduled action {ActionType} completed for job {JobId}.", job.ActionType, job.JobId);
    }
}

public sealed class PlatformScheduledActionHandler(PlatformActionDispatcher dispatcher) : IScheduledJobActionHandler
{
    public string ActionType => "event.audit";

    public Task ExecuteAsync(ScheduledJobActionContext context, CancellationToken cancellationToken) =>
        dispatcher.DispatchAsync(new PlatformEvent(Guid.CreateVersion7(), context.TenantId,
            context.ActionType, context.Payload.GetRawText(), context.TriggeredAtUtc), cancellationToken);
}

public sealed class ScheduledNotificationActionHandler(PlatformActionDispatcher dispatcher) : IScheduledJobActionHandler
{
    public string ActionType => "notification.create";

    public Task ExecuteAsync(ScheduledJobActionContext context, CancellationToken cancellationToken) =>
        dispatcher.DispatchAsync(new PlatformEvent(Guid.CreateVersion7(), context.TenantId,
            context.ActionType, context.Payload.GetRawText(), context.TriggeredAtUtc), cancellationToken);
}

public sealed class ScheduledWorkflowActionHandler(TenantWorkflowRuntimeService workflows) : IScheduledJobActionHandler
{
    public string ActionType => "workflow.start";

    public async Task ExecuteAsync(ScheduledJobActionContext context, CancellationToken cancellationToken)
    {
        if (!context.Payload.TryGetProperty("workflowId", out var workflowValue) || !workflowValue.TryGetGuid(out var workflowId) ||
            !context.Payload.TryGetProperty("recordId", out var recordValue) || !recordValue.TryGetGuid(out var recordId))
            throw new InvalidOperationException("Workflow actions require valid workflowId and recordId values.");
        await workflows.StartScheduled(new StartWorkflowRequest(context.TenantId, workflowId, recordId), cancellationToken);
    }
}

public sealed class ScheduledReportActionHandler(
    BuilderDbContext db,
    DynamicQueryService query,
    ScheduledReportArtifactStore artifacts,
    ScheduledMailService mail,
    ILogger<ScheduledReportActionHandler> logger) : IScheduledJobActionHandler
{
    private const int MaximumExportRows = 5000;
    private const int MaximumExportCharacters = 10_000_000;
    public string ActionType => "report.generate";

    public async Task ExecuteAsync(ScheduledJobActionContext context, CancellationToken cancellationToken)
    {
        var reportKey = context.Payload.TryGetProperty("reportKey", out var keyValue) && keyValue.ValueKind == JsonValueKind.String
            ? keyValue.GetString()?.Trim() : null;
        var reports = db.Reports.AsNoTracking().Where(x => x.TenantId == context.TenantId && x.ApplicationId == context.ApplicationId);
        if (!string.IsNullOrWhiteSpace(reportKey)) reports = reports.Where(x => x.Key == reportKey);
        var selectedReports = await reports.OrderBy(x => x.Name).Take(100).ToListAsync(cancellationToken);
        if (selectedReports.Count == 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(reportKey)
                ? "There are no reports in this application to generate."
                : $"Report '{reportKey}' was not found in this application.");

        var csv = new StringBuilder("Report key,Record ID,Created at,Data JSON\r\n");
        var exportedRows = 0;
        var systemUser = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, BuilderRoleNames.SystemAdmin)], "scheduled-job"));
        foreach (var report in selectedReports)
        {
            using var definition = JsonDocument.Parse(report.DefinitionJson);
            var root = definition.RootElement;
            var filters = root.TryGetProperty("filters", out var list) && list.ValueKind == JsonValueKind.Array
                ? list.EnumerateArray().Where(x => x.TryGetProperty("field", out _) && x.TryGetProperty("operator", out _))
                    .Select(x => new DynamicFilter(x.GetProperty("field").GetString() ?? "", x.GetProperty("operator").GetString() ?? "eq",
                        x.TryGetProperty("value", out var value) ? value.ToString() : null)).ToArray()
                : null;
            var sortField = root.TryGetProperty("sortField", out var sort) ? sort.GetString() : null;
            var descending = root.TryGetProperty("descending", out var desc) && desc.ValueKind == JsonValueKind.True;
            var pageSize = 200;
            var firstPage = await query.Query(new DynamicQueryRequest(context.TenantId, report.ObjectId,
                filters, sortField, descending, 1, pageSize), cancellationToken, systemUser);
            var rowLimit = Math.Min(firstPage.Total, MaximumExportRows - exportedRows);
            var reportRowsWritten = 0;
            for (var page = 1; page <= (rowLimit + pageSize - 1) / pageSize; page++)
            {
                var result = page == 1 ? firstPage : await query.Query(new DynamicQueryRequest(context.TenantId,
                    report.ObjectId, filters, sortField, descending, page, pageSize), cancellationToken, systemUser);
                foreach (var record in result.Items.Take(rowLimit - reportRowsWritten))
                {
                    csv.Append(Csv(report.Key)).Append(',').Append(Csv(record.Id.ToString("D"))).Append(',')
                        .Append(Csv(record.CreatedAt.ToString("O"))).Append(',').Append(Csv(record.DataJson)).Append("\r\n");
                    exportedRows++;
                    reportRowsWritten++;
                    if (csv.Length > MaximumExportCharacters)
                        throw new InvalidOperationException("Scheduled CSV exceeded the 10 MB export limit.");
                }
                if (exportedRows >= MaximumExportRows) break;
            }
            logger.LogInformation("Scheduled report {ReportId} ({ReportName}) queried {RowCount} rows for job {JobId}.",
                report.Id, report.Name, reportRowsWritten, context.JobId);
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        await artifacts.SaveLatestAsync(context.JobId, bytes, cancellationToken);
        logger.LogInformation("Saved scheduled report CSV for job {JobId} with {RowCount} rows.", context.JobId, exportedRows);

        if (context.Payload.TryGetProperty("email", out var email))
        {
            var recipient = email.GetProperty("to").GetString()!;
            var subject = email.TryGetProperty("subject", out var subjectValue) && subjectValue.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(subjectValue.GetString())
                ? subjectValue.GetString()! : $"Scheduled report: {context.JobName}";
            var body = email.TryGetProperty("body", out var bodyValue) && bodyValue.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(bodyValue.GetString())
                ? bodyValue.GetString()! : "The scheduled report is attached as a CSV file.";
            await mail.SendAttachmentAsync(recipient, subject, body, $"{SafeFileName(context.JobName)}-{context.TriggeredAtUtc:yyyyMMdd-HHmmss}.csv",
                "text/csv", bytes, cancellationToken);
            logger.LogInformation("Emailed scheduled report for job {JobId} to {Recipient}.", context.JobId, recipient);
        }
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
    private static string SafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var safe = new string(value.Select(character => invalid.Contains(character) ? '-' : character).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(safe) ? "scheduled-report" : safe;
    }
}

public sealed class ScheduledEmailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Sorte";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
}

public sealed class ScheduledMailService(IOptions<ScheduledEmailOptions> options)
{
    public async Task SendAsync(string recipient, string subject, string body, bool isHtml, CancellationToken cancellationToken)
    {
        var smtp = options.Value;
        if (string.IsNullOrWhiteSpace(smtp.Host) || !MailAddress.TryCreate(smtp.FromAddress, out _))
            throw new InvalidOperationException("Scheduled email is not configured. Set ScheduledJobs:Mail:Host and FromAddress.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtp.FromName, smtp.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = isHtml ? new BodyBuilder { HtmlBody = body }.ToMessageBody() : new TextPart("plain") { Text = body };
        await SendMessageAsync(smtp, message, cancellationToken);
    }

    public async Task SendAttachmentAsync(string recipient, string subject, string body, string fileName, string contentType,
        byte[] attachment, CancellationToken cancellationToken)
    {
        var smtp = options.Value;
        if (string.IsNullOrWhiteSpace(smtp.Host) || !MailAddress.TryCreate(smtp.FromAddress, out _))
            throw new InvalidOperationException("Scheduled email is not configured. Set ScheduledJobs:Mail:Host and FromAddress.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtp.FromName, smtp.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder { TextBody = body };
        bodyBuilder.Attachments.Add(fileName, attachment, MimeKit.ContentType.Parse(contentType));
        message.Body = bodyBuilder.ToMessageBody();
        await SendMessageAsync(smtp, message, cancellationToken);
    }

    private static async Task SendMessageAsync(ScheduledEmailOptions smtp, MimeMessage message, CancellationToken cancellationToken)
    {
        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(smtp.Host, smtp.Port,
            smtp.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
        if (!string.IsNullOrWhiteSpace(smtp.Username))
            await client.AuthenticateAsync(smtp.Username, smtp.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}

public sealed class ScheduledEmailActionHandler(ScheduledMailService mail) : IScheduledJobActionHandler
{
    public string ActionType => "email.send";

    public Task ExecuteAsync(ScheduledJobActionContext context, CancellationToken cancellationToken)
    {
        var recipient = RequiredString(context.Payload, "to");
        var subject = RequiredString(context.Payload, "subject");
        var body = RequiredString(context.Payload, "body");
        var isHtml = context.Payload.TryGetProperty("isHtml", out var html) && html.ValueKind == JsonValueKind.True;
        return mail.SendAsync(recipient, subject, body, isHtml, cancellationToken);
    }

    internal static string RequiredString(JsonElement payload, string name) =>
        payload.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()! : throw new InvalidOperationException($"Scheduled action payload requires '{name}'.");
}

public sealed class ScheduledApiOptions
{
    public string[] AllowedHosts { get; set; } = [];
    public int MaximumResponseBytes { get; set; } = 1_000_000;
}

public sealed class ScheduledApiActionHandler(
    HttpClient client,
    IOptions<ScheduledApiOptions> options,
    ILogger<ScheduledApiActionHandler> logger) : IScheduledJobActionHandler
{
    private static readonly HashSet<string> AllowedMethods = ["GET", "POST", "PUT", "PATCH", "DELETE"];
    public string ActionType => "http.request";

    public async Task ExecuteAsync(ScheduledJobActionContext context, CancellationToken cancellationToken)
    {
        var rawUrl = ScheduledEmailActionHandler.RequiredString(context.Payload, "url");

        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(uri.UserInfo) || IPAddress.TryParse(uri.Host, out _))
            throw new InvalidOperationException("Scheduled API calls require an HTTPS URL with a DNS hostname and no embedded credentials.");

        if (!options.Value.AllowedHosts.Any(host => string.Equals(host, uri.Host, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"The scheduled API host '{uri.Host}' is not listed in ScheduledJobs:Api:AllowedHosts.");

        var method = ScheduledEmailActionHandler.RequiredString(context.Payload, "method").ToUpperInvariant();

        if (!AllowedMethods.Contains(method)) throw new InvalidOperationException("The scheduled API method is not supported.");

        using var request = new HttpRequestMessage(new HttpMethod(method), uri);

        if (context.Payload.TryGetProperty("body", out var body) && body.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            request.Content = new StringContent(body.ValueKind == JsonValueKind.String ? body.GetString()! : body.GetRawText(), Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if ((int)response.StatusCode is >= 300 and < 400)
            throw new InvalidOperationException("Scheduled API calls do not follow redirects; configure the final HTTPS endpoint directly.");
        if ((long?)response.Content.Headers.ContentLength > options.Value.MaximumResponseBytes)
            throw new InvalidOperationException("The scheduled API response exceeded the configured size limit.");
        response.EnsureSuccessStatusCode();
        logger.LogInformation("Scheduled API call completed with HTTP {StatusCode} for job {JobId}.", (int)response.StatusCode, context.JobId);
    }
}
