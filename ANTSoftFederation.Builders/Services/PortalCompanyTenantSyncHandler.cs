using SorteFederation.Shared.Realtime;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SorteFederation.Builders.Services;

/// <summary>Mirrors Portal's company identity into the Builder tenant directory.</summary>
public sealed class PortalCompanyTenantSyncHandler(BuilderDbContext db, ILogger<PortalCompanyTenantSyncHandler> logger) : IRealtimeEventHandler
{
    public async Task HandleAsync(RealtimeEvent message, CancellationToken cancellationToken)
    {
        if (message.EventName == RabbitMQTopics.TenantSubscriptionChanged)
        {
            await SyncSubscriptionAsync(message, cancellationToken);
            return;
        }
        if (message.EventName is not (RabbitMQTopics.CompanyCreated or RabbitMQTopics.CompanyUpdated or RabbitMQTopics.CompanyDeleted)) return;

        var companyId = ReadCompanyId(message.Data);
        var name = ReadName(message.Data);
        if (companyId is null || companyId == Guid.Empty || message.TenantId != companyId.Value ||
            (message.EventName != RabbitMQTopics.CompanyDeleted && string.IsNullOrWhiteSpace(name)))
        {
            logger.LogWarning("Ignoring invalid Portal company event {EventId} ({EventName}); the company ID must match the event tenant ID.", message.EventId, message.EventName);
            return;
        }

        var tenant = await db.Tenants.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == companyId.Value, cancellationToken);
        if (tenant is null)
        {
            tenant = new Tenant { TenantId = companyId.Value, Name = name ?? string.Empty };
            db.Tenants.Add(tenant);
        }

        if (message.EventName == RabbitMQTopics.CompanyDeleted)
            tenant.RecordState = RecordState.Deleted;
        else
        {
            tenant.Name = name!.Trim();
            tenant.RecordState = RecordState.Active;
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Synchronized Portal company {CompanyId} ({EventName}) into Builder tenant directory.", companyId.Value, message.EventName);
    }

    private async Task SyncSubscriptionAsync(RealtimeEvent message, CancellationToken cancellationToken)
    {
        if (message.Data is not JsonElement data || data.ValueKind != JsonValueKind.Object ||
            !TryGuid(data, "tenantId", out var tenantId) || tenantId == Guid.Empty || tenantId != message.TenantId ||
            !data.TryGetProperty("hasActiveSubscription", out var activeValue) ||
            activeValue.ValueKind is not (JsonValueKind.True or JsonValueKind.False) ||
            !data.TryGetProperty("maxApplications", out var appsValue) || !appsValue.TryGetInt32(out var maxApps) ||
            !data.TryGetProperty("maxUsers", out var usersValue) || !usersValue.TryGetInt32(out var maxUsers))
        {
            logger.LogWarning("Ignoring invalid tenant subscription event {EventId}.", message.EventId);
            return;
        }

        var tenant = await db.Tenants.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (tenant is null)
        {
            tenant = new Tenant
            {
                TenantId = tenantId,
                Name = data.TryGetProperty("tenantName", out var nameValue) && nameValue.ValueKind == JsonValueKind.String
                    ? nameValue.GetString() ?? string.Empty : string.Empty,
                RecordState = RecordState.Active
            };
            db.Tenants.Add(tenant);
        }
        tenant.HasActiveSubscription = activeValue.GetBoolean();
        tenant.SubscriptionMaxApplications = maxApps;
        tenant.SubscriptionMaxUsers = maxUsers;
        tenant.SubscriptionStartsOn = data.TryGetProperty("startsOn", out var startsValue) && startsValue.ValueKind == JsonValueKind.String &&
            DateOnly.TryParse(startsValue.GetString(), out var startsOn) ? startsOn : null;
        tenant.SubscriptionEndsOn = data.TryGetProperty("endsOn", out var endsValue) && endsValue.ValueKind == JsonValueKind.String &&
            DateOnly.TryParse(endsValue.GetString(), out var endsOn) ? endsOn : null;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Synchronized subscription limits for tenant {TenantId}.", tenantId);
    }

    private static bool TryGuid(JsonElement data, string name, out Guid value)
    {
        value = Guid.Empty;
        return data.TryGetProperty(name, out var element) && element.ValueKind == JsonValueKind.String && Guid.TryParse(element.GetString(), out value);
    }

    private static Guid? ReadCompanyId(JsonElement? data)
    {
        if (data is not JsonElement value || value.ValueKind != JsonValueKind.Object) return null;
        if (!value.TryGetProperty("companyId", out var id) && !value.TryGetProperty("CompanyId", out id)) return null;
        return id.ValueKind == JsonValueKind.String && Guid.TryParse(id.GetString(), out var companyId) ? companyId : null;
    }

    private static string? ReadName(JsonElement? data)
    {
        if (data is not JsonElement value || value.ValueKind != JsonValueKind.Object) return null;
        if (!value.TryGetProperty("name", out var name) && !value.TryGetProperty("Name", out name)) return null;
        return name.ValueKind == JsonValueKind.String ? name.GetString() : null;
    }
}
