using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data.Domain;
using SorteFederation.Builders.Services;
using System.Security.Claims;

namespace SorteFederation.Builders.Endpoints;

public sealed class NotificationEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapNotificationEndpoints(endpoints);

public static IEndpointRouteBuilder MapNotificationEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/notifications", (CreateNotificationRequest request, ClaimsPrincipal user, NotificationService service, CancellationToken token) => service.Create(request, user, token)).WithTags("Notifications");
        endpoints.MapGet("/notifications", (Guid tenantId, string? recipientId, bool unreadOnly, ClaimsPrincipal user, NotificationService service, CancellationToken token) =>
        {
            var ownId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name ?? throw new UnauthorizedAccessException();
            var effectiveRecipient = user.IsInRole(BuilderRoleNames.SystemAdmin) && !string.IsNullOrWhiteSpace(recipientId) ? recipientId.Trim() : ownId;
            return service.List(tenantId, effectiveRecipient, unreadOnly, token);
        }).WithTags("Notifications");
        endpoints.MapPost("/notifications/{id:guid}/read", (Guid id, Guid tenantId, ClaimsPrincipal user, NotificationService service, CancellationToken token) => service.MarkRead(tenantId, id, user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name ?? throw new UnauthorizedAccessException(), token)).WithTags("Notifications");
        return endpoints;
    }
}
