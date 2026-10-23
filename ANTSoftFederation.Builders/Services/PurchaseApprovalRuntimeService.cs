using System.Security.Claims;
using System.Text.Json;
using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data.Domain;
using SorteFederation.Builders.Data.Enums;
using SorteFederation.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.Builders.Services;

/// <summary>Runs the built-in Purchase request approval workflow with server-owned state transitions.</summary>
public sealed class PurchaseApprovalRuntimeService(
    BuilderDbContext metadata,
    TenantRuntimeContextFactory factory,
    BuilderAuthorizationService authorization,
    IHttpContextAccessor accessor,
    AuditService audit,
    IMailKitService mailer)
{
    public async Task<bool> IsApprovalWorkflow(Guid tenantId, Guid workflowId, CancellationToken token) =>
        await metadata.Workflows.AsNoTracking().AnyAsync(x => x.Id == workflowId && x.TenantId == tenantId &&
            x.DefinitionJson.Contains("\"purchaseApproval\""), token);

    public async Task<bool> IsApprovalTask(Guid tenantId, Guid taskId, CancellationToken token)
    {
        await using var runtime = await factory.CreateAsync(tenantId, token);
        var id = await runtime.WorkflowTasks.AsNoTracking().Where(x => x.Id == taskId && x.TenantId == tenantId)
            .Select(x => (Guid?)x.WorkflowInstance.WorkflowId).SingleOrDefaultAsync(token);
        return id is Guid value && await IsApprovalWorkflow(tenantId, value, token);
    }

    public async Task<BuilderWorkflowInstance> Submit(SubmitPurchaseApprovalRequest request, CancellationToken token)
    {
        var user = CurrentUser();
        var workflow = await metadata.Workflows.AsNoTracking().SingleOrDefaultAsync(x =>
            x.Id == request.WorkflowId && x.TenantId == request.TenantId && x.Status == DefinitionStatus.Published, token)
            ?? throw new InvalidOperationException("Only a published approval workflow can be submitted.");
        if (!PurchaseApprovalDefinition.TryParse(workflow.DefinitionJson, out var definition) || definition is null)
            throw new InvalidOperationException("The selected workflow is not a Purchase approval workflow.");

        var application = await metadata.Applications.AsNoTracking().SingleOrDefaultAsync(x =>
            x.Id == workflow.ApplicationId && x.TenantId == request.TenantId && x.Status == DefinitionStatus.Published, token)
            ?? throw new InvalidOperationException("The Purchase application must be published before requests can be submitted.");
        var objectInfo = await metadata.Objects.AsNoTracking().Where(x =>
                x.TenantId == request.TenantId && x.ApplicationId == application.Id && x.Key == definition.ObjectKey)
            .Select(x => new { x.Id, x.Key }).SingleOrDefaultAsync(token)
            ?? throw new InvalidOperationException("The approval workflow's request data object is missing.");

        await using var runtime = await factory.CreateAsync(request.TenantId, token);
        var record = await runtime.Records.SingleOrDefaultAsync(x => x.Id == request.RecordId && x.TenantId == request.TenantId, token)
            ?? throw new KeyNotFoundException("Purchase request was not found.");
        if (record.ObjectId != objectInfo.Id) throw new InvalidOperationException("Only a Purchase request in this application can be submitted.");
        await authorization.EnsureRecordAccess(request.TenantId, record.ObjectId, record.Id, "read", user, token);

        using var data = JsonDocument.Parse(record.DataJson);
        var status = PurchaseApprovalRecordRules.FindField(data.RootElement, definition.StatusField);
        if (!string.Equals(status, definition.SubmittedStatus, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(status, definition.RejectedStatus, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only a draft or rejected request can be submitted.");
        ValidateRequest(data.RootElement, definition, user);
        var requesterEmployee = definition.Routing is null
            ? null
            : PurchaseApprovalRecordRules.FindField(data.RootElement, definition.Routing.RequesterEmployeeField);
        var amount = decimal.Parse(PurchaseApprovalRecordRules.FindField(data.RootElement, "total-estimate")!);
        var routing = definition.Routing is null
            ? null
            : await ResolveApprover(runtime, request.TenantId, application.Id, definition.Routing, requesterEmployee, amount, token);

        var instance = await runtime.WorkflowInstances.Include(x => x.Tasks).SingleOrDefaultAsync(x =>
            x.TenantId == request.TenantId && x.WorkflowId == workflow.Id && x.RecordId == record.Id, token);
        if (instance is not null && instance.Status == WorkflowInstanceStatus.Running)
            throw new InvalidOperationException("This request is already in approval.");
        if (instance is null)
        {
            instance = new BuilderWorkflowInstance { TenantId = request.TenantId, WorkflowId = workflow.Id, RecordId = record.Id };
            runtime.WorkflowInstances.Add(instance);
        }
        else
        {
            runtime.WorkflowTasks.RemoveRange(instance.Tasks);
            instance.Tasks.Clear();
            instance.Status = WorkflowInstanceStatus.Running;
            instance.CurrentState = "In approval";
            instance.Version++;
            instance.LastError = null;
            instance.UpdatedAt = DateTimeOffset.UtcNow;
        }
        instance.CurrentState = "In approval";
        if (definition.Routing is not null && routing is null)
        {
            instance.Status = WorkflowInstanceStatus.Completed;
            instance.CurrentState = "Cancelled";
            instance.Version++;
            instance.UpdatedAt = DateTimeOffset.UtcNow;
            record.DataJson = PurchaseApprovalRecordRules.SetStatus(record.DataJson, definition.StatusField, "Cancelled");
            record.Version++;
            record.UpdatedAt = DateTimeOffset.UtcNow;
            await runtime.SaveChangesAsync(token);
            await WriteAudit(request.TenantId, "purchase.approval.cancelled-no-approver", instance.Id,
                new { WorkflowId = workflow.Id, RecordId = record.Id, reason = "No eligible approver in the organization hierarchy." }, user, token);
            await NotifyRequester(application.Id, request.TenantId, definition.Routing, requesterEmployee,
                "Purchase request cancelled", "Your purchase request was cancelled because no eligible approver was available.", token);
            return instance;
        }

        if (definition.Routing is not null)
        {
            instance.Tasks.Add(new BuilderWorkflowTask
            {
                TenantId = request.TenantId,
                Name = $"Approval — {routing!.EmployeeName}",
                AssignedTo = routing.PortalUserId,
                LastError = JsonSerializer.Serialize(new { employeeId = routing.EmployeeId, employeeName = routing.EmployeeName, authorityLimit = routing.ApprovalLimit }),
                Activated = true
            });
        }
        else foreach (var step in definition.Steps)
        {
            instance.Tasks.Add(new BuilderWorkflowTask
            {
                TenantId = request.TenantId,
                Name = step.Name,
                AssignedTo = step.AssignedTo,
                DependsOn = step.DependsOn,
                DueAt = step.DueHours is int hours ? DateTimeOffset.UtcNow.AddHours(hours) : null,
                Activated = string.IsNullOrWhiteSpace(step.DependsOn)
            });
        }
        record.DataJson = PurchaseApprovalRecordRules.SetStatus(record.DataJson, definition.StatusField, definition.PendingStatus);
        record.Version++;
        record.UpdatedAt = DateTimeOffset.UtcNow;
        await runtime.SaveChangesAsync(token);
        await WriteAudit(request.TenantId, "purchase.approval.submitted", instance.Id,
            new { WorkflowId = workflow.Id, WorkflowVersion = workflow.Version, RecordId = record.Id, application.PublishedVersion }, user, token);
        if (definition.Routing is not null)
            await NotifyRequester(application.Id, request.TenantId, definition.Routing, requesterEmployee,
                "Purchase request submitted", $"Your request is awaiting approval from {routing!.EmployeeName}.", token);
        return instance;
    }

    public async Task<bool> Decide(Guid tenantId, Guid taskId, PurchaseApprovalDecisionRequest request, CancellationToken token)
    {
        var decision = request.Decision?.Trim().ToLowerInvariant();
        if (decision is not ("approve" or "reject")) throw new ArgumentException("Decision must be approve or reject.");
        var comment = request.Comment?.Trim();
        if (decision == "reject" && string.IsNullOrWhiteSpace(comment)) throw new ArgumentException("A comment is required when rejecting a request.");
        if (comment?.Length > 2000) throw new ArgumentException("Comments cannot exceed 2,000 characters.");

        var user = CurrentUser();
        await using var runtime = await factory.CreateAsync(tenantId, token);
        var task = await runtime.WorkflowTasks.Include(x => x.WorkflowInstance)
            .SingleOrDefaultAsync(x => x.Id == taskId && x.TenantId == tenantId, token);
        if (task is null) return false;
        if (task.Completed) throw new InvalidOperationException("This approval task has already been decided.");
        if (!task.Activated || task.WorkflowInstance.Status != WorkflowInstanceStatus.Running)
            throw new InvalidOperationException("This approval task is not currently active.");

        var workflow = await metadata.Workflows.AsNoTracking().SingleOrDefaultAsync(x =>
            x.Id == task.WorkflowInstance.WorkflowId && x.TenantId == tenantId && x.Status == DefinitionStatus.Published, token)
            ?? throw new InvalidOperationException("The published approval workflow was not found.");
        if (!PurchaseApprovalDefinition.TryParse(workflow.DefinitionJson, out var definition) || definition is null)
            throw new InvalidOperationException("This task is not part of a Purchase approval workflow.");
        var record = await runtime.Records.SingleOrDefaultAsync(x => x.Id == task.WorkflowInstance.RecordId && x.TenantId == tenantId, token)
            ?? throw new KeyNotFoundException("Purchase request was not found.");
        var objectInfo = await metadata.Objects.AsNoTracking().Where(x => x.Id == record.ObjectId && x.TenantId == tenantId)
            .Select(x => new { x.Key, x.ApplicationId }).SingleOrDefaultAsync(token);
        if (objectInfo?.Key != definition.ObjectKey || objectInfo.ApplicationId != workflow.ApplicationId)
            throw new InvalidOperationException("The approval task is not attached to a Purchase request in this application.");
        await authorization.EnsureRecordAccess(tenantId, record.ObjectId, record.Id, "read", user, token);
        EnsureAssignee(user, task.AssignedTo);

        using var data = JsonDocument.Parse(record.DataJson);
        var requester = PurchaseApprovalRecordRules.FindField(data.RootElement, definition.RequesterField);
        var actorId = PurchaseApprovalRecordRules.ActorId(user);
        var privileged = IsPrivileged(user);
        var requesterEmployeeId = definition.Routing is null ? null :
            PurchaseApprovalRecordRules.FindField(data.RootElement, definition.Routing.RequesterEmployeeField);
        var employeePortalId = definition.Routing is null ? null :
            await GetEmployeePortalUserId(workflow.ApplicationId, tenantId, definition.Routing, requesterEmployeeId, token);
        if (!privileged && actorId is not null &&
            (string.Equals(requester, actorId, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(employeePortalId, actorId, StringComparison.OrdinalIgnoreCase)))
            throw new UnauthorizedAccessException("A requester cannot approve or reject their own request.");

        task.Completed = true;
        task.CompletedAt = DateTimeOffset.UtcNow;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        task.LastError = JsonSerializer.Serialize(new { decision, comment, decidedBy = actorId, workflowVersion = workflow.Version });
        var allTasks = await runtime.WorkflowTasks.Where(x => x.TenantId == tenantId && x.WorkflowInstanceId == task.WorkflowInstanceId).ToListAsync(token);
        if (decision == "reject")
        {
            foreach (var pending in allTasks.Where(x => !x.Completed)) { pending.Completed = true; pending.CompletedAt = DateTimeOffset.UtcNow; pending.LastError = "Closed because the request was rejected."; }
            task.WorkflowInstance.Status = WorkflowInstanceStatus.Completed;
            task.WorkflowInstance.CurrentState = definition.RejectedStatus;
            record.DataJson = PurchaseApprovalRecordRules.SetStatus(record.DataJson, definition.StatusField, definition.RejectedStatus);
        }
        else
        {
            foreach (var dependent in allTasks.Where(x => !x.Activated && !x.Completed && !string.IsNullOrWhiteSpace(x.DependsOn)))
            {
                var deps = dependent.DependsOn!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (deps.Length > 0 && deps.All(name => allTasks.Any(x => x.Completed && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))) dependent.Activated = true;
            }
            if (allTasks.All(x => x.Completed))
            {
                task.WorkflowInstance.Status = WorkflowInstanceStatus.Completed;
                task.WorkflowInstance.CurrentState = definition.ApprovedStatus;
                record.DataJson = PurchaseApprovalRecordRules.SetStatus(record.DataJson, definition.StatusField, definition.ApprovedStatus);
            }
            else task.WorkflowInstance.CurrentState = $"Approval {allTasks.Count(x => x.Completed) + 1} of {allTasks.Count}";
        }
        task.WorkflowInstance.Version++;
        task.WorkflowInstance.UpdatedAt = DateTimeOffset.UtcNow;
        record.Version++;
        record.UpdatedAt = DateTimeOffset.UtcNow;
        await runtime.SaveChangesAsync(token);
        var auditDecision = decision == "approve" ? "approved" : "rejected";
        await WriteAudit(tenantId, $"purchase.approval.{auditDecision}", task.WorkflowInstanceId,
            new { TaskId = task.Id, task.Name, RecordId = record.Id, comment, actorId, WorkflowId = workflow.Id, WorkflowVersion = workflow.Version, status = task.WorkflowInstance.CurrentState }, user, token);
        if (definition.Routing is not null)
        {
            using var requestData = JsonDocument.Parse(record.DataJson);
            await NotifyRequester(workflow.ApplicationId, tenantId, definition.Routing,
                PurchaseApprovalRecordRules.FindField(requestData.RootElement, definition.Routing.RequesterEmployeeField),
                decision == "approve" ? "Purchase request approved" : "Purchase request rejected",
                decision == "approve" ? "Your purchase request has been approved." : $"Your purchase request was rejected. {comment}", token);
        }
        return true;
    }

    private async Task<ResolvedApprover?> ResolveApprover(TenantRuntimeDbContext runtime, Guid tenantId, Guid applicationId,
        PurchaseApprovalRouting routing, string? requesterEmployeeId, decimal amount, CancellationToken token)
    {
        if (!Guid.TryParse(requesterEmployeeId, out var requesterId)) return null;
        var objectIds = await metadata.Objects.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId &&
                (x.Key == routing.EmployeeObjectKey || x.Key == routing.DepartmentObjectKey || x.Key == routing.AbsenceObjectKey || x.Key == routing.DelegationObjectKey))
            .Select(x => new { x.Id, x.Key }).ToListAsync(token);
        var ids = objectIds.ToDictionary(x => x.Key, x => x.Id, StringComparer.OrdinalIgnoreCase);
        if (!ids.TryGetValue(routing.EmployeeObjectKey, out var employeeObjectId) ||
            !ids.TryGetValue(routing.DepartmentObjectKey, out var departmentObjectId)) return null;
        var relatedObjectIds = ids.Values.Distinct().ToArray();
        var records = await runtime.Records.AsNoTracking().Where(x => x.TenantId == tenantId && relatedObjectIds.Contains(x.ObjectId))
            .Select(x => new { x.Id, x.ObjectId, x.DataJson }).ToListAsync(token);
        var employees = records.Where(x => x.ObjectId == employeeObjectId)
            .Select(x => new EmployeeRecord(x.Id, x.DataJson)).ToDictionary(x => x.Id);
        if (!employees.TryGetValue(requesterId, out var requester) || !requester.Active) return null;
        var departmentId = requester.IdValue(routing.DepartmentField);
        var department = departmentId is Guid id ? records.FirstOrDefault(x => x.Id == id && x.ObjectId == departmentObjectId) : null;
        var headId = department is null ? null : new EmployeeRecord(department.Id, department.DataJson).IdValue(routing.DepartmentHeadField);
        if (headId is null) return null;

        var absenceObjectId = ids.GetValueOrDefault(routing.AbsenceObjectKey);
        var delegationObjectId = ids.GetValueOrDefault(routing.DelegationObjectKey);
        var absences = records.Where(x => absenceObjectId != Guid.Empty && x.ObjectId == absenceObjectId)
            .Select(x => new AbsenceRecord(x.DataJson)).ToArray();
        var delegations = records.Where(x => delegationObjectId != Guid.Empty && x.ObjectId == delegationObjectId)
            .Select(x => new DelegationRecord(x.DataJson)).ToArray();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var visited = new HashSet<Guid> { requesterId };
        var currentId = headId.Value;
        for (var depth = 0; depth < 32 && visited.Add(currentId); depth++)
        {
            if (!employees.TryGetValue(currentId, out var employee) || !employee.Active) break;
            var activeAbsence = absences.FirstOrDefault(x => x.EmployeeId == currentId && x.IsActive(today));
            var candidate = employee;
            if (activeAbsence is not null)
            {
                var coverage = delegations.FirstOrDefault(x => x.AbsentEmployeeId == currentId && x.IsActive(today) &&
                    x.Start >= activeAbsence.Start && x.End <= activeAbsence.End && x.Start <= x.End);
                if (coverage is null || !employees.TryGetValue(coverage.SubstituteEmployeeId, out candidate) || !candidate.Active ||
                    absences.Any(x => x.EmployeeId == candidate.Id && x.IsActive(today)))
                {
                    currentId = employee.IdValue(routing.ManagerField) ?? Guid.Empty;
                    if (currentId == Guid.Empty) break;
                    continue;
                }
            }
            if (candidate.Id != requesterId && candidate.ApprovalLimit >= amount && !string.IsNullOrWhiteSpace(candidate.PortalUserId))
                return new(candidate.Id, candidate.FullName, candidate.PortalUserId, candidate.ApprovalLimit);
            currentId = employee.IdValue(routing.ManagerField) ?? Guid.Empty;
            if (currentId == Guid.Empty) break;
        }
        return null;
    }

    private async Task NotifyRequester(Guid applicationId, Guid tenantId, PurchaseApprovalRouting routing, string? employeeId,
        string subject, string message, CancellationToken token)
    {
        if (!Guid.TryParse(employeeId, out var id)) return;
        var employeeObjectId = await metadata.Objects.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.Key == routing.EmployeeObjectKey)
            .Select(x => (Guid?)x.Id).SingleOrDefaultAsync(token);
        if (employeeObjectId is null) return;
        await using var runtime = await factory.CreateAsync(tenantId, token);
        var json = await runtime.Records.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == employeeObjectId && x.Id == id)
            .Select(x => x.DataJson).SingleOrDefaultAsync(token);
        if (json is null) return;
        using var document = JsonDocument.Parse(json);
        var email = PurchaseApprovalRecordRules.FindField(document.RootElement, "work-email");
        if (string.IsNullOrWhiteSpace(email)) return;
        await mailer.SendAsync(new MailData(email, subject, $"<p>{System.Net.WebUtility.HtmlEncode(message)}</p>"), token);
    }

    private async Task<string?> GetEmployeePortalUserId(Guid applicationId, Guid tenantId, PurchaseApprovalRouting routing,
        string? employeeId, CancellationToken token)
    {
        if (!Guid.TryParse(employeeId, out var id)) return null;
        var employeeObjectId = await metadata.Objects.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.Key == routing.EmployeeObjectKey)
            .Select(x => (Guid?)x.Id).SingleOrDefaultAsync(token);
        if (employeeObjectId is null) return null;
        await using var runtime = await factory.CreateAsync(tenantId, token);
        var json = await runtime.Records.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == employeeObjectId && x.Id == id)
            .Select(x => x.DataJson).SingleOrDefaultAsync(token);
        if (json is null) return null;
        using var document = JsonDocument.Parse(json);
        return PurchaseApprovalRecordRules.FindField(document.RootElement, routing.PortalUserIdField);
    }

    private sealed record ResolvedApprover(Guid EmployeeId, string EmployeeName, string PortalUserId, decimal ApprovalLimit);
    private sealed class EmployeeRecord(Guid id, string json)
    {
        private readonly JsonElement data = JsonDocument.Parse(json).RootElement.Clone();
        public Guid Id { get; } = id;
        public bool Active => bool.TryParse(PurchaseApprovalRecordRules.FindField(data, "active"), out var active) && active;
        public decimal ApprovalLimit => decimal.TryParse(PurchaseApprovalRecordRules.FindField(data, "approval-limit"), out var limit) ? limit : 0;
        public string? PortalUserId => PurchaseApprovalRecordRules.FindField(data, "portal-user-id");
        public string FullName => $"{PurchaseApprovalRecordRules.FindField(data, "name")} {PurchaseApprovalRecordRules.FindField(data, "surname")}".Trim();
        public Guid? IdValue(string key) => Guid.TryParse(PurchaseApprovalRecordRules.FindField(data, key), out var value) ? value : null;
    }
    private sealed class AbsenceRecord(string json)
    {
        private readonly JsonElement data = JsonDocument.Parse(json).RootElement.Clone();
        public Guid? EmployeeId => Guid.TryParse(PurchaseApprovalRecordRules.FindField(data, "employee"), out var value) ? value : null;
        public DateOnly Start => DateOnly.TryParse(PurchaseApprovalRecordRules.FindField(data, "start-date"), out var value) ? value : DateOnly.MinValue;
        public DateOnly End => DateOnly.TryParse(PurchaseApprovalRecordRules.FindField(data, "end-date"), out var value) ? value : DateOnly.MinValue;
        public bool IsActive(DateOnly today) => PurchaseApprovalRecordRules.FindField(data, "status") is { } status &&
            (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase) || string.Equals(status, "Scheduled", StringComparison.OrdinalIgnoreCase)) &&
            string.Equals(PurchaseApprovalRecordRules.FindField(data, "absence-type"), "Vacation", StringComparison.OrdinalIgnoreCase) && Start <= today && End >= today;
    }
    private sealed class DelegationRecord(string json)
    {
        private readonly JsonElement data = JsonDocument.Parse(json).RootElement.Clone();
        public Guid? AbsentEmployeeId => Guid.TryParse(PurchaseApprovalRecordRules.FindField(data, "absent-employee"), out var value) ? value : null;
        public Guid SubstituteEmployeeId => Guid.TryParse(PurchaseApprovalRecordRules.FindField(data, "substitute-employee"), out var value) ? value : Guid.Empty;
        public DateOnly Start => DateOnly.TryParse(PurchaseApprovalRecordRules.FindField(data, "start-date"), out var value) ? value : DateOnly.MinValue;
        public DateOnly End => DateOnly.TryParse(PurchaseApprovalRecordRules.FindField(data, "end-date"), out var value) ? value : DateOnly.MinValue;
        public bool IsActive(DateOnly today) => string.Equals(PurchaseApprovalRecordRules.FindField(data, "status"), "Active", StringComparison.OrdinalIgnoreCase) && Start <= today && End >= today;
    }

    private static void ValidateRequest(JsonElement data, PurchaseApprovalDefinition definition, ClaimsPrincipal user)
    {
        foreach (var field in new[] { "title", "request-type", definition.RequesterField, "total-estimate", "justification" })
            if (string.IsNullOrWhiteSpace(PurchaseApprovalRecordRules.FindField(data, field)))
                throw new ArgumentException($"Complete the required '{field}' field before submitting.");
        var requester = PurchaseApprovalRecordRules.FindField(data, definition.RequesterField);
        var actor = PurchaseApprovalRecordRules.ActorId(user);
        if (!IsPrivileged(user) && actor is not null && !string.Equals(requester, actor, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("You can submit only a request assigned to you as requester.");
        if (!decimal.TryParse(PurchaseApprovalRecordRules.FindField(data, "total-estimate"), out var total) || total <= 0)
            throw new ArgumentException("Estimated total must be greater than zero.");
    }

    private static void EnsureAssignee(ClaimsPrincipal user, string? assignedTo)
    {
        if (IsPrivileged(user) || string.IsNullOrWhiteSpace(assignedTo)) return;
        var actorId = PurchaseApprovalRecordRules.ActorId(user);
        var roles = user.FindAll(ClaimTypes.Role).Concat(user.FindAll("role")).Select(x => x.Value);
        if (!string.Equals(assignedTo, actorId, StringComparison.OrdinalIgnoreCase) && !roles.Contains(assignedTo, StringComparer.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("This approval task is assigned to another user or role.");
    }

    private static bool IsPrivileged(ClaimsPrincipal user) => user.IsInRole(BuilderRoleNames.SystemAdmin) || user.IsInRole(BuilderRoleNames.TenantAdmin);
    private ClaimsPrincipal CurrentUser() => accessor.HttpContext?.User ?? new ClaimsPrincipal();
    private async Task WriteAudit(Guid tenantId, string type, Guid resourceId, object details, ClaimsPrincipal user, CancellationToken token)
    {
        await audit.Record(new CreateAuditEventRequest(tenantId, type, "purchase-approval", resourceId, JsonSerializer.Serialize(details)),
            user, accessor.HttpContext?.TraceIdentifier, token);
    }
}
