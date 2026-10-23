
using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Services;

public sealed class PlatformRuntimeService(BuilderDbContext db)
{
    public async Task<BuilderWorkflowTransition> AddTransition(CreateWorkflowTransitionRequest r, CancellationToken t)
    {
        if (!await db.Workflows.AnyAsync(x => x.Id == r.WorkflowId && x.TenantId == r.TenantId, t)) throw new KeyNotFoundException("Workflow was not found.");
        if (string.IsNullOrWhiteSpace(r.FromState) || string.IsNullOrWhiteSpace(r.ToState) || string.IsNullOrWhiteSpace(r.Action)) throw new ArgumentException("Workflow transition states and action are required.");
        if (!string.IsNullOrWhiteSpace(r.ConditionJson)) try { using var _ = JsonDocument.Parse(r.ConditionJson); } catch (JsonException) { throw new ArgumentException("ConditionJson must be valid JSON."); }
        var e = new BuilderWorkflowTransition { TenantId = r.TenantId, WorkflowId = r.WorkflowId, FromState = r.FromState.Trim(), ToState = r.ToState.Trim(), Action = r.Action.Trim(), ConditionJson = r.ConditionJson };
        db.WorkflowTransitions.Add(e); await db.SaveChangesAsync(t); return e;
    }
    public async Task<BuilderReport> AddReport(CreateReportRequest r, CancellationToken t) { if (!await db.Applications.AnyAsync(x => x.Id == r.ApplicationId && x.TenantId == r.TenantId, t) || !await db.Objects.AnyAsync(x => x.Id == r.ObjectId && x.ApplicationId == r.ApplicationId && x.TenantId == r.TenantId, t)) throw new KeyNotFoundException("Application or object was not found for this tenant."); var e = new BuilderReport { TenantId = r.TenantId, ApplicationId = r.ApplicationId, ObjectId = r.ObjectId, Key = r.Key.Trim(), Name = r.Name.Trim(), DefinitionJson = r.DefinitionJson }; db.Reports.Add(e); await db.SaveChangesAsync(t); return e; }

    public async Task<BuilderDashboard> AddDashboard(CreateDashboardRequest r, CancellationToken t) { if (!await db.Applications.AnyAsync(x => x.Id == r.ApplicationId && x.TenantId == r.TenantId, t)) throw new KeyNotFoundException("Application was not found for this tenant."); var e = new BuilderDashboard { TenantId = r.TenantId, ApplicationId = r.ApplicationId, Key = r.Key.Trim(), Name = r.Name.Trim(), LayoutJson = r.LayoutJson }; db.Dashboards.Add(e); await db.SaveChangesAsync(t); return e; }

    public async Task<BuilderDashboardWidget> AddWidget(CreateDashboardWidgetRequest r, CancellationToken t) { if (!await db.Dashboards.AnyAsync(x => x.Id == r.DashboardId && x.TenantId == r.TenantId, t)) throw new KeyNotFoundException("Dashboard was not found."); if (string.IsNullOrWhiteSpace(r.Key) || string.IsNullOrWhiteSpace(r.Type)) throw new ArgumentException("Widget key and type are required."); try { using var configuration = JsonDocument.Parse(r.ConfigurationJson); if (configuration.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Widget configuration must be a JSON object."); } catch (JsonException) { throw new ArgumentException("ConfigurationJson must be valid JSON."); } var e = new BuilderDashboardWidget { TenantId = r.TenantId, DashboardId = r.DashboardId, Key = r.Key.Trim(), Type = r.Type.Trim(), ConfigurationJson = r.ConfigurationJson, SortOrder = r.SortOrder }; db.DashboardWidgets.Add(e); await db.SaveChangesAsync(t); return e; }

    public async Task<BuilderConnectorCredential> AddCredential(CreateCredentialRequest r, CancellationToken t)
    {
        if (!await db.Connectors.AnyAsync(x => x.Id == r.ConnectorId && x.TenantId == r.TenantId, t))
            throw new KeyNotFoundException("Connector was not found for this tenant.");
        if (string.IsNullOrWhiteSpace(r.SecretReference)) throw new ArgumentException("SecretReference is required.");
        var scheme = r.Scheme.Trim().ToLowerInvariant();
        if (scheme is not ("bearer" or "api-key")) throw new ArgumentException("Credential scheme must be bearer or api-key.");
        var e = new BuilderConnectorCredential { TenantId = r.TenantId, ConnectorId = r.ConnectorId, SecretReference = r.SecretReference.Trim(), Scheme = scheme };
        db.ConnectorCredentials.Add(e); await db.SaveChangesAsync(t); return e;
    }
    public async Task<bool> RevokeCredential(Guid tenantId, Guid credentialId, CancellationToken t)
    {
        var credential = await db.ConnectorCredentials.SingleOrDefaultAsync(x => x.Id == credentialId && x.TenantId == tenantId, t);
        if (credential is null) return false;
        credential.Active = false;
        credential.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(t);
        return true;
    }
    public async Task<BuilderFeatureFlag> AddFeatureFlag(CreateFeatureFlagRequest r, CancellationToken t) { if (!await db.Applications.AnyAsync(x => x.Id == r.ApplicationId && x.TenantId == r.TenantId, t)) throw new KeyNotFoundException("Application was not found for this tenant."); if (string.IsNullOrWhiteSpace(r.Key)) throw new ArgumentException("Feature flag key is required."); if (!string.IsNullOrWhiteSpace(r.RulesJson)) try { using var _ = JsonDocument.Parse(r.RulesJson); } catch (JsonException) { throw new ArgumentException("RulesJson must be valid JSON."); } var e=new BuilderFeatureFlag{TenantId=r.TenantId,ApplicationId=r.ApplicationId,Key=r.Key.Trim(),Enabled=r.Enabled,RulesJson=r.RulesJson};db.FeatureFlags.Add(e);await db.SaveChangesAsync(t);return e; }
    public async Task<IReadOnlyList<BuilderRecord>> Report(Guid tenantId, Guid objectId, string? field, string? value, CancellationToken t)
    {
        var q = db.Records.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == objectId);
        if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(value)) q = q.Where(x => x.DataJson.Contains(field) && x.DataJson.Contains(value));
        return await q.OrderByDescending(x => x.UpdatedAt).Take(1000).ToListAsync(t);
    }

}
