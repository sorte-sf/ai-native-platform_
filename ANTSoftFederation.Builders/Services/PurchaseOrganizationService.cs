using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.Builders.Services;

/// <summary>Validates workforce and vacation-coverage records used by Purchase approval routing.</summary>
public sealed class PurchaseOrganizationService(BuilderDbContext metadata, TenantRuntimeContextFactory factory)
{
    public async Task ValidateAsync(Guid tenantId, Guid objectId, string dataJson, string? originalJson, Guid? recordId, CancellationToken token)
    {
        var target = await (from objectInfo in metadata.Objects.AsNoTracking()
            join app in metadata.Applications.AsNoTracking() on objectInfo.ApplicationId equals app.Id
            where objectInfo.Id == objectId && objectInfo.TenantId == tenantId
            select new { objectInfo.Key, objectInfo.ApplicationId, app.TemplateId }).SingleOrDefaultAsync(token);
        if (target?.TemplateId is not Guid templateId ||
            !await metadata.Templates.AsNoTracking().AnyAsync(x => x.Id == templateId && x.Key == "purchase-starter", token)) return;

        using var dataDocument = JsonDocument.Parse(dataJson);
        var data = dataDocument.RootElement;
        if (target.Key == "employee-absences")
        {
            var start = Date(data, "start-date");
            var end = Date(data, "end-date");
            if (start > end) throw new ArgumentException("An employee absence must end on or after its start date.");
            if (!string.Equals(Value(data, "absence-type"), "Vacation", StringComparison.OrdinalIgnoreCase)) return;
        }
        else if (target.Key == "approval-delegations")
        {
            var start = Date(data, "start-date");
            var end = Date(data, "end-date");
            if (start > end) throw new ArgumentException("Approval coverage must end on or after its start date.");
            if (!Guid.TryParse(Value(data, "absent-employee"), out var absentId) ||
                !Guid.TryParse(Value(data, "substitute-employee"), out var substituteId) || absentId == substituteId)
                throw new ArgumentException("Choose two different employees for the temporary coverage assignment.");

            await using var runtime = await factory.CreateAsync(tenantId, token);
            var objectIds = await metadata.Objects.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == target.ApplicationId &&
                    (x.Key == "employees" || x.Key == "employee-absences"))
                .Select(x => new { x.Id, x.Key }).ToListAsync(token);
            var employeeObjectId = objectIds.FirstOrDefault(x => x.Key == "employees")?.Id;
            var absenceObjectId = objectIds.FirstOrDefault(x => x.Key == "employee-absences")?.Id;
            if (employeeObjectId is null || absenceObjectId is null) throw new InvalidOperationException("The Purchase organization tables are missing.");
            var absenceJson = await runtime.Records.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == absenceObjectId && x.Id == absentId)
                .Select(x => x.DataJson).SingleOrDefaultAsync(token);
            if (absenceJson is null) throw new ArgumentException("The absent employee has no matching absence record.");
            using var absenceDocument = JsonDocument.Parse(absenceJson);
            var absence = absenceDocument.RootElement;
            if (!string.Equals(Value(absence, "absence-type"), "Vacation", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Value(absence, "status"), "Cancelled", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Temporary approval coverage can only be assigned for a non-cancelled vacation.");
            if (start < Date(absence, "start-date") || end > Date(absence, "end-date"))
                throw new ArgumentException("Coverage dates must fall entirely within the employee's recorded vacation dates.");
            var substituteJson = await runtime.Records.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == employeeObjectId && x.Id == substituteId)
                .Select(x => x.DataJson).SingleOrDefaultAsync(token);
            if (substituteJson is null) throw new ArgumentException("The substitute must be an employee in this application.");
            using var substituteDocument = JsonDocument.Parse(substituteJson);
            if (!bool.TryParse(Value(substituteDocument.RootElement, "active"), out var active) || !active)
                throw new ArgumentException("The substitute employee must be active.");
        }
        else if (target.Key == "employees" && Guid.TryParse(Value(data, "manager"), out var managerId))
        {
            if (managerId == Guid.Empty) return;
            await using var runtime = await factory.CreateAsync(tenantId, token);
            var employeeObjectId = await metadata.Objects.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == target.ApplicationId && x.Key == "employees")
                .Select(x => (Guid?)x.Id).SingleOrDefaultAsync(token);
            if (employeeObjectId is null) return;
            var employees = await runtime.Records.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == employeeObjectId)
                .Select(x => new { x.Id, x.DataJson }).ToListAsync(token);
            var visited = new HashSet<Guid>();
            var nextId = managerId;
            for (var depth = 0; depth < 64; depth++)
            {
                if (recordId is Guid self && nextId == self) throw new ArgumentException("An employee cannot report to themselves or to someone below them in the reporting hierarchy.");
                if (!visited.Add(nextId)) throw new ArgumentException("The reporting hierarchy cannot contain a cycle.");
                var manager = employees.FirstOrDefault(x => x.Id == nextId);
                if (manager is null) break;
                if (!Guid.TryParse(Value(Parse(manager.DataJson), "manager"), out nextId)) break;
            }
        }

        if (target.Key == "employee-absences" && originalJson is not null)
            await EnsureExistingCoverageStillFits(tenantId, target.ApplicationId, data, token);
    }

    private async Task EnsureExistingCoverageStillFits(Guid tenantId, Guid applicationId, JsonElement changedAbsence, CancellationToken token)
    {
        if (!Guid.TryParse(Value(changedAbsence, "employee"), out var employeeId)) return;
        var delegationObjectId = await metadata.Objects.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.Key == "approval-delegations")
            .Select(x => (Guid?)x.Id).SingleOrDefaultAsync(token);
        if (delegationObjectId is null) return;
        await using var runtime = await factory.CreateAsync(tenantId, token);
        var delegations = await runtime.Records.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == delegationObjectId)
            .Select(x => x.DataJson).ToListAsync(token);
        var changedStart = Date(changedAbsence, "start-date");
        var changedEnd = Date(changedAbsence, "end-date");
        foreach (var json in delegations)
        {
            using var document = JsonDocument.Parse(json);
            var delegation = document.RootElement;
            if (!Guid.TryParse(Value(delegation, "absent-employee"), out var assignedEmployee) || assignedEmployee != employeeId ||
                string.Equals(Value(delegation, "status"), "Cancelled", StringComparison.OrdinalIgnoreCase)) continue;
            if (!string.Equals(Value(changedAbsence, "absence-type"), "Vacation", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Value(changedAbsence, "status"), "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                Date(delegation, "start-date") < changedStart || Date(delegation, "end-date") > changedEnd)
                throw new ArgumentException("Cancel the employee's approval coverage before cancelling, changing, or shortening the vacation.");
        }
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string? Value(JsonElement data, string key) =>
        data.ValueKind == JsonValueKind.Object && data.TryGetProperty(key, out var value)
            ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()
            : null;

    private static DateOnly Date(JsonElement data, string key) =>
        DateOnly.TryParse(Value(data, key), out var value) ? value : throw new ArgumentException($"A valid {key} date is required.");
}
