using SorteFederation.Builders.Data.Domain;
using System.Text.Json;

namespace SorteFederation.Builders.Services;

public sealed class PlatformActionDispatcher(
    NotificationService notifications,
    TenantWorkflowRuntimeService workflows,
    ConnectorExecutionService connectors,
    AuditService audit)
{
    public async Task DispatchAsync(PlatformEvent value, CancellationToken token)
    {
        using var payload = JsonDocument.Parse(value.PayloadJson);
        var root = payload.RootElement;
        switch (value.Type.ToLowerInvariant())
        {
            case "event.publish":
            case "event.audit":
                await audit.Record(new SorteFederation.Builders.Contracts.CreateAuditEventRequest(
                    value.TenantId, value.Type, "platform-event", value.Id, value.PayloadJson),
                    new System.Security.Claims.ClaimsPrincipal(), null, token);
                break;
            case "notification.send":
            case "notification.create":
                await notifications.Create(new SorteFederation.Builders.Contracts.CreateNotificationRequest(
                    value.TenantId,
                    Required(root, "recipientId"),
                    Required(root, "title"),
                    Required(root, "message"),
                    Optional(root, "type"), Optional(root, "actionUrl")),
                    Elevated(), token);
                break;
            case "notification.created":
                // NotificationService already persisted the notification. This event is an audit/transport hook.
                break;
            case "workflow.start":
                await workflows.Start(new SorteFederation.Builders.Contracts.StartWorkflowRequest(
                    value.TenantId, RequiredGuid(root, "workflowId"), RequiredGuid(root, "recordId")), token);
                break;
            case "connector.execute":
                var connectorId = RequiredGuid(root, "connectorId");
                var connectorResult = await connectors.Execute(new SorteFederation.Builders.Contracts.ExecuteConnectorRequest(
                    value.TenantId, connectorId, Optional(root, "payloadJson")), token);
                if (connectorResult is Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult status && status.StatusCode >= 400)
                    throw new InvalidOperationException($"Connector action returned HTTP {status.StatusCode}.");
                break;
            default:
                if (value.Type.StartsWith("connector.", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Connector actions must be invoked through the connector execution endpoint with an authenticated caller.");
                throw new InvalidOperationException($"Unsupported platform action '{value.Type}'.");
        }
    }

    private static string Required(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()! : throw new InvalidOperationException($"Action payload requires '{name}'.");
    private static string? Optional(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static Guid RequiredGuid(JsonElement root, string name) => Guid.TryParse(Required(root, name), out var value) ? value : throw new InvalidOperationException($"Action payload requires a valid '{name}'.");
    private static System.Security.Claims.ClaimsPrincipal Elevated() => new(new System.Security.Claims.ClaimsIdentity(
        [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, BuilderRoleNames.SystemAdmin)], "platform-action"));
}
