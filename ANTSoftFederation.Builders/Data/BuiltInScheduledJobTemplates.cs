namespace SorteFederation.Builders.Data;

/// <summary>
/// Seeds the Builder's built-in scheduler presets.
/// </summary>
public static class BuiltInScheduledJobTemplates
{
    public static async Task SeedAsync(BuilderDbContext db, CancellationToken token = default)
    {
        var jobTemplates = new (string Key, string Name, string Category, string Description, string Cron, string ActionType, string ActionJson)[]
        {
            ("job-health-check", "Health Check", "Monitoring", "Every 15 minutes · Ping a service and raise an alarm if it is still unhealthy.", "0 */15 * * * *", "event.audit", "{\"event\":\"health-check\"}"),
            ("job-session-cleanup", "Session Cleanup", "Maintenance", "Every 6 hours · Drop expired sessions and stale tokens.", "0 0 */6 * * *", "event.audit", "{\"event\":\"session-cleanup\"}"),
            ("job-hourly-sync", "Hourly Sync", "Integration", "Every hour · Pull changes from an external system.", "0 0 * * * *", "event.audit", "{\"event\":\"hourly-sync\"}"),
            ("job-nightly-backup", "Nightly Backup", "Maintenance", "At 02:30 · Take the nightly copy before cleanup jobs start.", "0 30 2 * * *", "event.audit", "{\"event\":\"nightly-backup\"}"),
            ("job-daily-cleanup", "Daily Cleanup", "Maintenance", "At 03:00 · Clear expired records, temporary files and stale logs.", "0 0 3 * * *", "event.audit", "{\"event\":\"daily-cleanup\"}"),
            ("job-daily-digest", "Daily Digest", "Notifications", "At 07:30 · Send yesterday's numbers.", "0 30 7 * * *", "notification.create", "{\"recipientId\":\"\",\"title\":\"Daily digest\",\"message\":\"Your daily digest is ready.\"}"),
            ("job-weekday-reminder", "Weekday Reminder", "Notifications", "At 09:00, Monday through Friday · Nudge people at the start of the workday.", "0 0 9 * * 1-5", "notification.create", "{\"recipientId\":\"\",\"title\":\"Weekday reminder\",\"message\":\"This is your scheduled reminder.\"}"),
            ("job-end-of-day-summary", "End Of Day Summary", "Reporting", "At 23:55 · Close the books on the day.", "0 55 23 * * *", "event.audit", "{\"event\":\"end-of-day-summary\"}"),
            ("job-weekly-report", "Weekly Report", "Reporting", "At 08:00 on Monday · Send the week's summary.", "0 0 8 * * 1", "event.audit", "{\"event\":\"weekly-report\"}"),
            ("job-invoice-run", "Invoice Run", "Finance", "At 09:00 on the 25th · Raise the month's invoices.", "0 0 9 25 * *", "event.audit", "{\"event\":\"invoice-run\"}"),
            ("job-monthly-close", "Monthly Close", "Finance", "At 01:00 on the first day of the month · Run the month-end job.", "0 0 1 1 * *", "event.audit", "{\"event\":\"monthly-close\"}"),
            ("job-quarterly-report", "Quarterly Report", "Reporting", "At 06:00 on the first day of Jan, Apr, Jul and Oct.", "0 0 6 1 1,4,7,10 *", "event.audit", "{\"event\":\"quarterly-report\"}")
        };

        foreach (var item in jobTemplates)
        {
            var jobTemplate = await db.Templates.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Key == item.Key, token);
            if (jobTemplate is { IsBuiltIn: false }) continue;
            var isNew = jobTemplate is null;
            jobTemplate ??= new BuilderTemplate { Key = item.Key, Name = item.Name, HtmlContent = "", ScssContent = "" };
            jobTemplate.Name = item.Name;
            jobTemplate.Category = $"Job Schedule: {item.Category}";
            jobTemplate.Description = item.Description;
            jobTemplate.IsBuiltIn = true;
            jobTemplate.ManifestVersion = 1;
            jobTemplate.ManifestJson = JsonSerializer.Serialize(new { kind = "job-schedule", cron = item.Cron, actionType = item.ActionType, actionJson = item.ActionJson }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            jobTemplate.HtmlContent = "";
            jobTemplate.ScssContent = "";
            jobTemplate.RecordState = RecordState.Active;
            if (isNew) db.Templates.Add(jobTemplate);
        }

        // Backfill old application references once where their saved schema or
        // release version identifies exactly one immutable Builder snapshot.
        var legacyApps = await db.Applications.IgnoreQueryFilters()
            .Where(x => x.TemplateId != null && x.TemplateReleaseId == null && x.TemplateVersion != null)
            .Select(x => new { x.Id, x.TemplateId, x.TemplateVersion, x.AppliedTemplateVersion })
            .ToListAsync(token);
        foreach (var app in legacyApps)
        {
            var candidates = db.TemplateReleases.Where(x => x.TemplateId == app.TemplateId);
            var matches = !string.IsNullOrWhiteSpace(app.AppliedTemplateVersion)
                ? await candidates.Where(x => x.ReleaseVersion == app.AppliedTemplateVersion).ToListAsync(token)
                : await candidates.Where(x => x.ManifestVersion == app.TemplateVersion).ToListAsync(token);
            if (matches.Count != 1) continue;
            await db.Applications.IgnoreQueryFilters().Where(x => x.Id == app.Id)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.TemplateReleaseId, matches[0].Id)
                    .SetProperty(x => x.AppliedTemplateVersion, matches[0].ReleaseVersion), token);
        }

        await db.SaveChangesAsync(token);
    }
}
