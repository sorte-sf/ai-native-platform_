using System.Globalization;
using System.Text.Json;
using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Services;

/// <summary>Deterministic interpreter for the workflow graph persisted by the Foblex designer.</summary>
public sealed class WorkflowGraphInterpreter
{
    public enum ParallelApprovalAction { AwaitingOthers, AdvanceApproved, AdvanceRejected }
    private readonly Dictionary<string, Node> _nodes;
    private readonly List<Edge> _edges;

    private WorkflowGraphInterpreter(Dictionary<string, Node> nodes, List<Edge> edges)
    {
        _nodes = nodes;
        _edges = edges;
    }

    public static bool HasGraph(string definitionJson)
    {
        using var json = JsonDocument.Parse(definitionJson);
        return json.RootElement.TryGetProperty("nodes", out var nodes) && nodes.ValueKind == JsonValueKind.Array;
    }

    public static WorkflowGraphInterpreter Parse(string definitionJson)
    {
        using var json = JsonDocument.Parse(definitionJson);
        var root = json.RootElement;
        if (!root.TryGetProperty("nodes", out var nodeArray) || nodeArray.ValueKind != JsonValueKind.Array ||
            !root.TryGetProperty("connections", out var edgeArray) || edgeArray.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Workflow graph requires nodes and connections arrays.");

        var nodes = new Dictionary<string, Node>(StringComparer.Ordinal);
        foreach (var element in nodeArray.EnumerateArray())
        {
            var id = String(element, "id");
            var type = String(element, "type");
            var title = String(element, "title");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Every workflow node requires an id, type and title.");
            if (type is not ("trigger" or "condition" or "approval" or "end"))
                throw new ArgumentException($"Workflow node type '{type}' is not executable yet. Email and alert nodes require a configured delivery action.");
            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (element.TryGetProperty("settings", out var values) && values.ValueKind == JsonValueKind.Object)
                foreach (var property in values.EnumerateObject()) settings[property.Name] = property.Value.ToString();
            if (!nodes.TryAdd(id, new(id, type, title, settings))) throw new ArgumentException($"Workflow node id '{id}' is duplicated.");
        }
        foreach (var approval in nodes.Values.Where(x => x.Type == "approval")) _ = ApprovalLevels(approval);

        var edges = new List<Edge>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var element in edgeArray.EnumerateArray())
        {
            var id = String(element, "id");
            var source = String(element, "source");
            var target = String(element, "target");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target) || !ids.Add(id) || !nodes.Keys.Any(key => source.StartsWith(key + "-", StringComparison.Ordinal)) || !nodes.Keys.Any(key => target.StartsWith(key + "-", StringComparison.Ordinal)))
                throw new ArgumentException("Every workflow connection must have a unique id and connect existing nodes.");
            var sourceNodeId = EndpointNode(source, nodes);
            var sourcePort = source[(sourceNodeId.Length + 1)..];
            var targetNodeId = EndpointNode(target, nodes);
            var targetPort = target[(targetNodeId.Length + 1)..];
            var sourceNode = nodes[sourceNodeId];
            var validSourcePort = sourceNode.Type switch
            {
                "trigger" => sourcePort == "out",
                "condition" => sourcePort is "yes" or "no",
                "approval" => sourcePort is "approved" or "rejected",
                _ => false
            };
            if (!validSourcePort || targetPort != "in") throw new ArgumentException("Workflow connections must use valid source and input ports.");
            edges.Add(new(id, source, target, String(element, "label")));
        }

        var starts = nodes.Values.Where(x => x.Type == "trigger").ToArray();
        if (starts.Length != 1) throw new ArgumentException("An executable workflow must have exactly one trigger.");
        if (!nodes.Values.Any(x => x.Type == "end")) throw new ArgumentException("An executable workflow must have at least one end node.");
        foreach (var node in nodes.Values)
        {
            var outgoing = edges.Where(x => EndpointNode(x.Source, nodes) == node.Id).ToArray();
            if (node.Type == "end" && outgoing.Length != 0) throw new ArgumentException($"End node '{node.Title}' cannot have outgoing connections.");
            if (node.Type is "trigger" or "approval" && !outgoing.Any(x => Output(node, x.Source) == "approved" || Output(node, x.Source) == "out"))
                throw new ArgumentException($"Node '{node.Title}' has no continuation connection.");
            if (node.Type == "condition" && (!outgoing.Any(x => Output(node, x.Source) == "yes") || !outgoing.Any(x => Output(node, x.Source) == "no")))
                throw new ArgumentException($"Condition '{node.Title}' must connect both yes and no branches.");
            if (node.Type is "trigger" or "approval" && outgoing.Count(x => Output(node, x.Source) == "approved" || Output(node, x.Source) == "out") != 1)
                throw new ArgumentException($"Node '{node.Title}' must have exactly one continuation for its completed outcome.");
            if (node.Type == "approval" && outgoing.Count(x => Output(node, x.Source) == "rejected") != 1)
                throw new ArgumentException($"Approval '{node.Title}' must connect both approved and rejected outcomes.");
            if (node.Type == "condition" && (outgoing.Count(x => Output(node, x.Source) == "yes") != 1 || outgoing.Count(x => Output(node, x.Source) == "no") != 1))
                throw new ArgumentException($"Condition '{node.Title}' must have exactly one connection for each branch.");
        }
        var reachable = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(); pending.Push(starts[0].Id);
        while (pending.TryPop(out var current))
        {
            if (!reachable.Add(current)) continue;
            foreach (var edge in edges.Where(x => EndpointNode(x.Source, nodes) == current)) pending.Push(EndpointNode(edge.Target, nodes));
        }
        if (reachable.Count != nodes.Count) throw new ArgumentException("Every workflow node must be reachable from its trigger.");
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        bool HasCycle(string id)
        {
            if (visited.Contains(id)) return false;
            if (!visiting.Add(id)) return true;
            foreach (var edge in edges.Where(x => EndpointNode(x.Source, nodes) == id))
                if (HasCycle(EndpointNode(edge.Target, nodes))) return true;
            visiting.Remove(id);
            visited.Add(id);
            return false;
        }
        if (nodes.Keys.Any(HasCycle)) throw new ArgumentException("Workflow graph cycles are not supported yet.");
        return new(nodes, edges);
    }

    public BuilderWorkflowTask? Start(Guid tenantId, string recordJson)
        => StartTasks(tenantId, recordJson).FirstOrDefault();

    public IReadOnlyList<BuilderWorkflowTask> StartTasks(Guid tenantId, string recordJson)
    {
        var trigger = _nodes.Values.Single(x => x.Type == "trigger");
        var next = Next(trigger, "out");
        return AdvanceTasks(tenantId, next, recordJson);
    }

    public BuilderWorkflowTask? Continue(Guid tenantId, string nodeId, string outcome, string recordJson)
    {
        if (!_nodes.TryGetValue(nodeId, out var node) || node.Type != "approval")
            throw new InvalidOperationException("The active workflow task does not map to an approval node.");
        var next = Next(node, outcome) ?? throw new InvalidOperationException($"Approval node '{node.Title}' has no '{outcome}' connection.");
        return AdvanceTasks(tenantId, next, recordJson).FirstOrDefault();
    }

    public BuilderWorkflowTask? ContinueApprovalLevel(Guid tenantId, string nodeId, int level, string outcome, string recordJson)
        => ContinueApprovalLevelTasks(tenantId, nodeId, level, outcome, recordJson).FirstOrDefault();

    public IReadOnlyList<BuilderWorkflowTask> ContinueApprovalLevelTasks(Guid tenantId, string nodeId, int level, string outcome, string recordJson)
    {
        if (!_nodes.TryGetValue(nodeId, out var node) || node.Type != "approval")
            throw new InvalidOperationException("The active workflow task does not map to an approval node.");
        if (outcome == "approved" && level + 1 < ApprovalLevels(node).Count)
            return CreateApprovalTasks(tenantId, node, level + 1);
        var next = Next(node, outcome) ?? throw new InvalidOperationException($"Approval node '{node.Title}' has no '{outcome}' connection.");
        return AdvanceTasks(tenantId, next, recordJson);
    }

    public (int Level, string GroupId, string Mode) ApprovalTaskContext(string? value)
    {
        var parts = (value ?? "").Split('|', StringSplitOptions.TrimEntries);
        var level = parts.Length > 0 && int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
        return (level, parts.Length > 1 ? parts[1] : "legacy", parts.Length > 2 ? parts[2] : "all");
    }

    public static ParallelApprovalAction ResolveParallelApproval(string mode, string outcome, int unresolvedApprovers)
    {
        if (outcome == "rejected") return ParallelApprovalAction.AdvanceRejected;
        if (outcome != "approved") throw new ArgumentException("Approval outcome must be approved or rejected.");
        if (mode == "any" || unresolvedApprovers <= 0) return ParallelApprovalAction.AdvanceApproved;
        if (mode == "all") return ParallelApprovalAction.AwaitingOthers;
        throw new ArgumentException("Approval mode must be all or any.");
    }

    public bool RequiresRequesterSeparation(string nodeId, int level)
    {
        if (!_nodes.TryGetValue(nodeId, out var node) || node.Type != "approval")
            throw new InvalidOperationException("The active workflow task does not map to an approval node.");
        var levels = ApprovalLevels(node);
        return levels[Math.Clamp(level, 0, levels.Count - 1)].PreventRequesterApproval;
    }

    private List<BuilderWorkflowTask> AdvanceTasks(Guid tenantId, Node? current, string recordJson)
    {
        using var data = JsonDocument.Parse(recordJson);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (current is not null)
        {
            if (!visited.Add(current.Id) || visited.Count > _nodes.Count)
                throw new InvalidOperationException("Workflow graph contains a cycle; cyclic execution is not supported.");
            if (current.Type == "end") return [];
            if (current.Type == "approval")
                return CreateApprovalTasks(tenantId, current, 0);
            if (current.Type == "condition")
            {
                var result = Evaluate(current, data.RootElement);
                current = Next(current, result ? "yes" : "no");
            }
            else current = Next(current, "out");
        }
        throw new InvalidOperationException("Workflow path ended without reaching an approval or end node.");
    }

    private static List<BuilderWorkflowTask> CreateApprovalTasks(Guid tenantId, Node node, int level)
    {
        var levels = ApprovalLevels(node);
        var current = levels[Math.Clamp(level, 0, levels.Count - 1)];
        var now = DateTimeOffset.UtcNow;
        var groupId = Guid.NewGuid().ToString("N");
        var joinKey = $"{level.ToString(CultureInfo.InvariantCulture)}|{groupId}|{current.Mode}";
        var assignees = current.AssignedTo!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return assignees.Select(assignee => new BuilderWorkflowTask
        {
            TenantId = tenantId,
            Name = levels.Count == 1 ? node.Title : current.Name,
            AssignedTo = assignee,
            DueAt = current.DueHours is null ? null : now.AddHours(current.DueHours.Value),
            EscalateAt = current.EscalationHours is null ? null : now.AddHours(current.EscalationHours.Value),
            EscalateTo = current.EscalateTo,
            BranchKey = node.Id,
            JoinKey = joinKey,
            Activated = true
        }).ToList();
    }

    private static List<ApprovalLevel> ApprovalLevels(Node node)
    {
        if (node.Settings.TryGetValue("approvalLevels", out var json))
        {
            try
            {
                var levels = JsonSerializer.Deserialize<List<ApprovalLevel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (levels is { Count: > 0 and <= 20 } && levels.All(IsValidApprovalLevel)) return levels;
                throw new ArgumentException($"Approval node '{node.Title}' must have 1–20 valid approval levels with an assignee for each level.");
            }
            catch (JsonException) { throw new ArgumentException($"Approval node '{node.Title}' has malformed approval levels."); }
        }
        var due = node.Settings.TryGetValue("dueHours", out var dueText) && int.TryParse(dueText, out var dueParsed) ? Math.Clamp(dueParsed, 1, 2160) : (int?)null;
        var escalation = node.Settings.TryGetValue("escalationHours", out var escalationText) && int.TryParse(escalationText, out var escalationParsed) ? Math.Clamp(escalationParsed, 1, 2160) : (int?)null;
        var assignedTo = node.Settings.GetValueOrDefault("assignedTo");
        if (string.IsNullOrWhiteSpace(assignedTo)) throw new ArgumentException($"Approval node '{node.Title}' requires an assigned user or role.");
        var mode = node.Settings.GetValueOrDefault("approvalMode") ?? "all";
        if (mode is not ("all" or "any")) throw new ArgumentException($"Approval node '{node.Title}' has an unsupported approval mode.");
        return [new(node.Title, assignedTo, due, escalation, node.Settings.GetValueOrDefault("escalateTo"), true, mode)];
    }

    private sealed record ApprovalLevel(string Name, string? AssignedTo, int? DueHours, int? EscalationHours, string? EscalateTo, bool PreventRequesterApproval = true, string Mode = "all");

    private static bool IsValidApprovalLevel(ApprovalLevel? level)
    {
        if (level is null || string.IsNullOrWhiteSpace(level.Name) || level.Name.Length > 160 ||
            string.IsNullOrWhiteSpace(level.AssignedTo) || level.AssignedTo.Length > 200 ||
            (level.DueHours is not null && level.DueHours is < 1 or > 2160) ||
            (level.EscalationHours is not null && level.EscalationHours is < 1 or > 2160) ||
            (level.EscalateTo?.Length ?? 0) > 200 || level.Mode is not ("all" or "any")) return false;
        var assignees = level.AssignedTo.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return assignees.Length is > 0 and <= 20 && assignees.Distinct(StringComparer.OrdinalIgnoreCase).Count() == assignees.Length;
    }

    private Node? Next(Node node, string output)
    {
        var edge = _edges.SingleOrDefault(x => EndpointNode(x.Source, _nodes) == node.Id && Output(node, x.Source) == output);
        return edge is null ? null : _nodes[EndpointNode(edge.Target, _nodes)];
    }

    private static bool Evaluate(Node node, JsonElement data)
    {
        if (!node.Settings.TryGetValue("field", out var field) || !node.Settings.TryGetValue("operator", out var op) || !data.TryGetProperty(field, out var actual))
            throw new InvalidOperationException($"Condition '{node.Title}' references a missing field or operator.");
        var expected = node.Settings.GetValueOrDefault("value") ?? string.Empty;
        var actualText = actual.ToString();
        if (decimal.TryParse(actualText, NumberStyles.Number, CultureInfo.InvariantCulture, out var left) &&
            decimal.TryParse(expected, NumberStyles.Number, CultureInfo.InvariantCulture, out var right))
            return op switch { "==" or "eq" => left == right, "!=" or "neq" => left != right, ">" or "gt" => left > right, ">=" or "gte" => left >= right, "<" or "lt" => left < right, "<=" or "lte" => left <= right, _ => throw new InvalidOperationException($"Unsupported condition operator '{op}'.") };
        var comparison = string.Compare(actualText, expected, StringComparison.OrdinalIgnoreCase);
        return op switch { "==" or "eq" => comparison == 0, "!=" or "neq" => comparison != 0, "contains" => actualText.Contains(expected, StringComparison.OrdinalIgnoreCase), ">" or "gt" => comparison > 0, ">=" or "gte" => comparison >= 0, "<" or "lt" => comparison < 0, "<=" or "lte" => comparison <= 0, _ => throw new InvalidOperationException($"Unsupported condition operator '{op}'.") };
    }

    private static string EndpointNode(string endpoint, IReadOnlyDictionary<string, Node> nodes) =>
        nodes.Keys.Where(key => endpoint.StartsWith(key + "-", StringComparison.Ordinal)).OrderByDescending(key => key.Length).First();
    private static string Output(Node node, string endpoint)
    {
        var port = endpoint.Length > node.Id.Length ? endpoint[(node.Id.Length + 1)..] : string.Empty;
        return node.Type switch
        {
            "condition" when port == "yes" => "yes",
            "condition" when port == "no" => "no",
            "approval" when port == "rejected" => "rejected",
            "approval" when port == "approved" => "approved",
            _ => "out"
        };
    }

    private static string? String(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private sealed record Node(string Id, string Type, string Title, Dictionary<string, string> Settings);
    private sealed record Edge(string Id, string Source, string Target, string? Label);
}
