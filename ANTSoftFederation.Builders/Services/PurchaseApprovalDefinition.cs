using System.Text.Json;

namespace SorteFederation.Builders.Services;

/// <summary>Typed contract for a purchase-request approval workflow embedded in a versioned workflow definition.</summary>
public sealed record PurchaseApprovalDefinition(string ObjectKey, string StatusField, string RequesterField,
    string SubmittedStatus, string PendingStatus, string ApprovedStatus, string RejectedStatus,
    IReadOnlyList<PurchaseApprovalStep> Steps, PurchaseApprovalRouting? Routing = null)
{
    public static bool TryParse(string definitionJson, out PurchaseApprovalDefinition? definition)
    {
        definition = null;
        try
        {
            using var document = JsonDocument.Parse(definitionJson);
            var root = document.RootElement;
            if (!root.TryGetProperty("purchaseApproval", out var approval) || approval.ValueKind != JsonValueKind.Object)
                return false;
            var objectKey = ReadString(approval, "objectKey");
            var statusField = ReadString(approval, "statusField");
            var requesterField = ReadString(approval, "requesterField");
            if (string.IsNullOrWhiteSpace(objectKey) || string.IsNullOrWhiteSpace(statusField) ||
                string.IsNullOrWhiteSpace(requesterField) || !root.TryGetProperty("tasks", out var tasks) ||
                tasks.ValueKind != JsonValueKind.Array || tasks.GetArrayLength() is < 1 or > 10)
                return false;

            var steps = new List<PurchaseApprovalStep>();
            var completedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var task in tasks.EnumerateArray())
            {
                if (task.ValueKind != JsonValueKind.Object || !task.TryGetProperty("name", out var nameElement)) return false;
                var name = nameElement.GetString()?.Trim();
                var assignedTo = task.TryGetProperty("assignedTo", out var assignedElement) ? assignedElement.GetString()?.Trim() : null;
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(assignedTo) || completedNames.Contains(name)) return false;
                var dependencyText = task.TryGetProperty("dependsOn", out var dependencies) && dependencies.ValueKind == JsonValueKind.String
                    ? dependencies.GetString()?.Trim() : null;
                var dependencyNames = string.IsNullOrWhiteSpace(dependencyText) ? Array.Empty<string>() : dependencyText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if ((!string.IsNullOrWhiteSpace(dependencyText) && dependencyNames.Length == 0) ||
                    dependencyNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() != dependencyNames.Length ||
                    dependencyNames.Any(dependency => !completedNames.Contains(dependency))) return false;
                steps.Add(new(name, assignedTo, dependencyText,
                    task.TryGetProperty("dueHours", out var hours) && hours.TryGetInt32(out var dueHours) ? Math.Clamp(dueHours, 1, 24 * 90) : null));
                completedNames.Add(name);
            }
            if (!steps.Any(x => string.IsNullOrWhiteSpace(x.DependsOn))) return false;
            definition = new(objectKey, statusField, requesterField,
                ReadString(approval, "submittedStatus") ?? "Draft",
                ReadString(approval, "pendingStatus") ?? "In approval",
                ReadString(approval, "approvedStatus") ?? "Approved",
                ReadString(approval, "rejectedStatus") ?? "Rejected", steps,
                approval.TryGetProperty("routing", out var routing) && routing.ValueKind == JsonValueKind.Object
                    ? new PurchaseApprovalRouting(
                        ReadString(routing, "employeeObjectKey") ?? "employees",
                        ReadString(routing, "departmentObjectKey") ?? "departments",
                        ReadString(routing, "absenceObjectKey") ?? "employee-absences",
                        ReadString(routing, "delegationObjectKey") ?? "approval-delegations",
                        ReadString(routing, "requesterEmployeeField") ?? "requester-employee",
                        ReadString(routing, "managerField") ?? "manager",
                        ReadString(routing, "approvalLimitField") ?? "approval-limit",
                        ReadString(routing, "portalUserIdField") ?? "portal-user-id",
                        ReadString(routing, "departmentField") ?? "department",
                        ReadString(routing, "departmentHeadField") ?? "department-head")
                    : null);
            return true;
        }
        catch (JsonException) { return false; }
    }

    private static string? ReadString(JsonElement value, string key) =>
        value.TryGetProperty(key, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString()?.Trim() : null;
}

public sealed record PurchaseApprovalStep(string Name, string AssignedTo, string? DependsOn, int? DueHours);
public sealed record PurchaseApprovalRouting(string EmployeeObjectKey, string DepartmentObjectKey, string AbsenceObjectKey,
    string DelegationObjectKey, string RequesterEmployeeField, string ManagerField, string ApprovalLimitField,
    string PortalUserIdField, string DepartmentField, string DepartmentHeadField);
