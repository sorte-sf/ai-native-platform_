
namespace SorteFederation.Realtime.Hubs;

[Authorize]
public sealed class RealtimeHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var principal = Context.User;
        if (principal is null)
        {
            Context.Abort();
            return;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            Context.Abort();
            return;
        }

        var tenantId = principal.FindFirstValue("tenantId")
            ?? principal.FindFirstValue("companyId")
            ?? principal.FindFirstValue("company_id");

        if (Guid.TryParse(tenantId, out var tenantGuid))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(tenantGuid));
            await Groups.AddToGroupAsync(Context.ConnectionId, TenantUserGroup(tenantGuid, userId));
        }

        await base.OnConnectedAsync();
    }

    public static string TenantGroup(Guid tenantId) => $"tenant:{tenantId:N}";
    public static string TenantUserGroup(Guid tenantId, string userId) => $"tenant:{tenantId:N}:user:{userId}";
}
