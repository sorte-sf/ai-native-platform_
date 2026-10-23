
namespace SorteFederation.Builders.Services;

public sealed class NotificationService(BuilderDbContext db)
{
    public async Task<BuilderNotification> Create(CreateNotificationRequest request, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientId) || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentException("RecipientId, title, and message are required.");
        var recipient = request.RecipientId.Trim();
        var ownId = user.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? user.Identity?.Name;
        var canTargetOthers = user.IsInRole(BuilderRoleNames.SystemAdmin) || user.IsInRole(BuilderRoleNames.TenantAdmin);
        if (!canTargetOthers && !string.Equals(recipient, ownId, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Users may only create notifications for themselves.");
        var title = request.Title.Trim();
        var message = request.Message.Trim();
        var type = request.Type?.Trim();
        var actionUrl = request.ActionUrl?.Trim();
        if (recipient.Length > 200 || title.Length > 300 || message.Length > 4000 || type?.Length > 100)
            throw new ArgumentException("Notification fields exceed their maximum length.");
        if (!string.IsNullOrWhiteSpace(actionUrl))
        {
            if (actionUrl.Length > 2048 || !Uri.TryCreate(actionUrl, UriKind.RelativeOrAbsolute, out var parsed) ||
                (parsed.IsAbsoluteUri && (!string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                                          string.IsNullOrWhiteSpace(parsed.Host))))
                throw new ArgumentException("ActionUrl must be a relative application path or an HTTPS URL.");
        }
        var notification = new BuilderNotification { TenantId = request.TenantId, RecipientId = recipient, Title = title, Message = message, Type = type, ActionUrl = actionUrl };
        db.Notifications.Add(notification);
        db.PlatformEvents.Add(new BuilderPlatformEvent
        {
            TenantId = request.TenantId,
            Type = "notification.created",
            Status = PlatformEventStatus.Queued,
            PayloadJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                notificationId = notification.Id,
                recipientId = recipient,
                title,
                message,
                type,
                actionUrl
            })
        });
        await db.SaveChangesAsync(token);
        return notification;
    }

    public Task<List<BuilderNotification>> List(Guid tenantId, string recipientId, bool unreadOnly, CancellationToken token) =>
        db.Notifications.AsNoTracking().Where(x => x.TenantId == tenantId && x.RecipientId == recipientId && (!unreadOnly || x.ReadAt == null)).OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(token);

    public async Task<bool> MarkRead(Guid tenantId, Guid id, string recipientId, CancellationToken token)
    {
        var notification = await db.Notifications.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id && x.RecipientId == recipientId, token);
        if (notification is null) return false;
        notification.ReadAt ??= DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);
        return true;
    }
}
