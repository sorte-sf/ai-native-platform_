using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SorteFederation.Builders.Services;

public sealed class RuleDefinitionService(BuilderDbContext db, RulesRuntimeService runtime, AuditService audit, IHttpContextAccessor accessor, PlatformEventQueue queue)
{
    public async Task TriggerAsync(Guid tenantId, Guid objectId, string eventName, Guid? recordId, string dataJson, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        var rules = await db.Rules.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Enabled && db.Objects.Any(o => o.Id == objectId && o.TenantId == tenantId && o.ApplicationId == x.ApplicationId))
            .ToListAsync(token);
        foreach (var rule in rules)
        {
            using var definition = JsonDocument.Parse(rule.DefinitionJson);
            if (!definition.RootElement.TryGetProperty("trigger", out var trigger) ||
                !string.Equals(trigger.GetString(), eventName, StringComparison.OrdinalIgnoreCase)) continue;
            var result = await runtime.EvaluateAsync(new EvaluateRulesRequest(tenantId, rule.DefinitionJson, dataJson), user, token);
            if (!result.Matched || result.Actions is null) continue;
            foreach (var action in result.Actions)
            {
                if (!IsApprovedAction(action)) throw new InvalidOperationException($"Rule action '{action}' is not approved.");
                var evt = new BuilderPlatformEvent { TenantId = tenantId, Type = action, Status = PlatformEventStatus.Queued,
                    PayloadJson = JsonSerializer.Serialize(new { ruleId = rule.Id, objectId, recordId, data = dataJson }) };
                db.PlatformEvents.Add(evt);
                await db.SaveChangesAsync(token);
                await queue.Enqueue(new PlatformEvent(evt.Id, evt.TenantId, evt.Type, evt.PayloadJson, evt.CreatedAt), token);
            }
            await RecordAudit(tenantId, "rule.triggered", rule.Id, token);
        }
    }
    public async Task<BuilderRule> CreateAsync(CreateRuleRequest request, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        EnsureDesigner(user);
        if (!await db.Applications.AnyAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token))
            throw new KeyNotFoundException("Application was not found for this tenant.");
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.DefinitionJson))
            throw new ArgumentException("Key, name, and definition are required.");
        if (request.Key.Trim().Length > 160 || request.Name.Trim().Length > 300 || request.DefinitionJson.Length > 1_000_000)
            throw new ArgumentException("Rule metadata exceeds the supported size limits.");
        if (await db.Rules.AnyAsync(x => x.TenantId == request.TenantId && x.ApplicationId == request.ApplicationId && x.Key == request.Key.Trim(), token))
            throw new InvalidOperationException("A rule with this key already exists in the application.");
        RulesRuntimeService.ValidateDefinition(request.DefinitionJson);
        var rule = new BuilderRule { TenantId = request.TenantId, ApplicationId = request.ApplicationId, Key = request.Key.Trim(), Name = request.Name.Trim(), DefinitionJson = request.DefinitionJson, Enabled = request.Enabled };
        db.Rules.Add(rule); await db.SaveChangesAsync(token); await RecordAudit(request.TenantId, "rule.created", rule.Id, token); return rule;
    }

    public Task<List<BuilderRule>> ListAsync(Guid tenantId, Guid applicationId, CancellationToken token) =>
        db.Rules.AsNoTracking().Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId).OrderBy(x => x.Name).ToListAsync(token);

    public async Task<BuilderRule> UpdateAsync(Guid tenantId, Guid ruleId, UpdateRuleRequest request, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        EnsureDesigner(user);
        var rule = await db.Rules.SingleOrDefaultAsync(x => x.Id == ruleId && x.TenantId == tenantId, token) ?? throw new KeyNotFoundException("Rule was not found.");
        if (request.ExpectedVersion > 0 && rule.Version != request.ExpectedVersion)
            throw new InvalidOperationException("The rule was changed by another user. Reload it before saving.");
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.DefinitionJson)) throw new ArgumentException("Key, name, and definition are required.");
        if (request.Key.Trim().Length > 160 || request.Name.Trim().Length > 300 || request.DefinitionJson.Length > 1_000_000) throw new ArgumentException("Rule metadata exceeds the supported size limits.");
        RulesRuntimeService.ValidateDefinition(request.DefinitionJson);
        rule.Key = request.Key.Trim(); rule.Name = request.Name.Trim(); rule.DefinitionJson = request.DefinitionJson; rule.Enabled = request.Enabled; rule.Version++; rule.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token); await RecordAudit(tenantId, "rule.updated", rule.Id, token); return rule;
    }

    public async Task<bool> DeleteAsync(Guid tenantId, Guid ruleId, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        EnsureDesigner(user);
        var rule = await db.Rules.SingleOrDefaultAsync(x => x.Id == ruleId && x.TenantId == tenantId, token);
        if (rule is null) return false;
        db.Rules.Remove(rule); await db.SaveChangesAsync(token); await RecordAudit(tenantId, "rule.deleted", ruleId, token); return true;
    }

    public async Task<EvaluateRulesResponse> EvaluateAsync(Guid tenantId, Guid ruleId, string dataJson, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        var rule = await db.Rules.AsNoTracking().SingleOrDefaultAsync(x => x.Id == ruleId && x.TenantId == tenantId, token) ?? throw new KeyNotFoundException("Rule was not found.");
        if (!rule.Enabled) return new EvaluateRulesResponse(false, ["The rule is disabled."]);
        return await runtime.EvaluateAsync(new EvaluateRulesRequest(tenantId, rule.DefinitionJson, dataJson), user, token);
    }

    public async Task<EvaluateRulesResponse> ExecuteAsync(ExecuteRuleRequest request, System.Security.Claims.ClaimsPrincipal user, CancellationToken token)
    {
        var result = await EvaluateAsync(request.TenantId, request.RuleId, request.DataJson, user, token);
        if (!result.Matched || result.Actions is null || result.Actions.Count == 0) return result;
        foreach (var action in result.Actions)
        {
            if (!IsApprovedAction(action))
                throw new InvalidOperationException($"Rule action '{action}' is not an approved platform action.");
            db.PlatformEvents.Add(new BuilderPlatformEvent { TenantId = request.TenantId, Type = action, Status = PlatformEventStatus.Pending, PayloadJson = JsonSerializer.Serialize(new { request.RuleId, data = request.DataJson }) });
        }
        await db.SaveChangesAsync(token);
        await RecordAudit(request.TenantId, "rules.actions.queued", request.RuleId, token);
        return result;
    }

    private static bool IsApprovedAction(string action) =>
        action.StartsWith("event.", StringComparison.OrdinalIgnoreCase) ||
        action.StartsWith("notification.", StringComparison.OrdinalIgnoreCase) ||
        action.StartsWith("workflow.", StringComparison.OrdinalIgnoreCase) ||
        action.StartsWith("connector.", StringComparison.OrdinalIgnoreCase);

    private Task RecordAudit(Guid tenantId, string eventType, Guid ruleId, CancellationToken token) =>
        audit.Record(new CreateAuditEventRequest(tenantId, eventType, "rule", ruleId, null), accessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal(), accessor.HttpContext?.TraceIdentifier, token);

    private static void EnsureDesigner(System.Security.Claims.ClaimsPrincipal user)
    {
        if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && !user.IsInRole(BuilderRoleNames.TenantAdmin) && !user.IsInRole(BuilderRoleNames.Developer))
            throw new UnauthorizedAccessException("Only administrators and developers may change rule definitions.");
    }
}
