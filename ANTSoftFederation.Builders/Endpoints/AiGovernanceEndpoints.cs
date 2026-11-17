using SorteFederation.Builders.Data.Entities;

namespace SorteFederation.Builders.Endpoints;

public sealed class AiGovernanceEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/ai").WithTags("AI Governance").RequireAuthorization();

        group.MapGet("/available-models", async (Guid tenantId, ClaimsPrincipal user, TenantAiPolicyService policies,
            IConfiguration configuration, CancellationToken token) =>
        {
            if (!BuilderApplicationCreationPolicy.CanGenerateForTenant(user, tenantId, out var authorizedTenant)) return Results.Forbid();
            var policy = await policies.ResolveAsync(authorizedTenant, "application-scaffold", token, ApplicationEditingPolicy.UserId(user));
            if (policy.Mode.Equals("Disabled", StringComparison.OrdinalIgnoreCase)) return Results.Ok(Array.Empty<object>());
            var options = new List<object>
            {
                new { provider = "ollama", model = configuration["AI:Local:Model"] ?? "qwen3:8b", mode = "Local", hasApiKey = false }
            };
            if (!policy.Mode.Equals("Local", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(policy.Model))
                options.Add(new { provider = policy.Provider, model = policy.Model, mode = policy.Mode, hasApiKey = !string.IsNullOrWhiteSpace(policy.ApiKeyReference) || !string.IsNullOrWhiteSpace(policy.ApiKey) });
            return Results.Ok(options);
        });

        group.MapPut("/personal-model", async (SavePersonalAiModelRequest request, ClaimsPrincipal user,
            TenantAiPolicyService policies, BuilderDbContext db, CancellationToken token) =>
        {
            if (!BuilderApplicationCreationPolicy.CanGenerateForTenant(user, request.TenantId, out var tenantId)) return Results.Forbid();
            var userId = ApplicationEditingPolicy.UserId(user);
            if (string.IsNullOrWhiteSpace(userId)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(request.Provider) || request.Provider.Length > 80 ||
                string.IsNullOrWhiteSpace(request.Model) || request.Model.Length > 160 ||
                !Uri.TryCreate(request.Endpoint, UriKind.Absolute, out var endpoint) || endpoint.Scheme != Uri.UriSchemeHttps ||
                !string.IsNullOrEmpty(endpoint.UserInfo) || !IsPublicEndpointHost(endpoint.Host) ||
                string.IsNullOrWhiteSpace(request.ApiKey) || request.ApiKey.Length > 2500)
                return Results.BadRequest("Provide a provider, model, HTTPS endpoint, and API key.");

            var saved = await db.AiUserModels.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, token);
            if (saved is null)
            {
                saved = new BuilderAiUserModel { TenantId = tenantId, UserId = userId };
                db.AiUserModels.Add(saved);
            }
            saved.Provider = request.Provider.Trim();
            saved.Endpoint = request.Endpoint!.Trim();
            saved.Model = request.Model.Trim();
            saved.ApiKey = request.ApiKey;
            saved.AuthScheme = request.AuthScheme?.Equals("api-key", StringComparison.OrdinalIgnoreCase) == true ? "api-key" : "bearer";
            saved.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return Results.Ok(new { provider = saved.Provider, model = saved.Model, hasApiKey = true });
        });

        group.MapGet("/policy", async (Guid tenantId, ClaimsPrincipal user, TenantAiPolicyService policies,
            CancellationToken token) =>
        {
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && !user.IsInRole(BuilderRoleNames.TenantAdmin))
                return Results.Forbid();
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && !OwnsTenant(user, tenantId)) return Results.Forbid();
            var policy = await policies.GetAsync(tenantId, token);
            return Results.Ok(new
            {
                policy.TenantId, policy.Mode, policy.Provider, policy.Endpoint, policy.Model,
                hasApiKey = !string.IsNullOrWhiteSpace(policy.ApiKeyReference) || !string.IsNullOrWhiteSpace(policy.ApiKey),
                policy.MonthlyTokenLimit, policy.MonthlyRequestLimit, policy.MaxOutputTokens
            });
        });

        group.MapPut("/policy", async (UpdateTenantAiPolicyRequest request, ClaimsPrincipal user,
            TenantAiPolicyService policies, BuilderDbContext db, CancellationToken token) =>
        {
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin))
            {
                if (!user.IsInRole(BuilderRoleNames.TenantAdmin) ||
                    !OwnsTenant(user, request.TenantId) ||
                    !request.Mode.Equals("Disabled", StringComparison.OrdinalIgnoreCase) &&
                    !request.Mode.Equals("Local", StringComparison.OrdinalIgnoreCase) &&
                    !request.Mode.Equals("CustomerCloud", StringComparison.OrdinalIgnoreCase))
                    return Results.Forbid();
                if (request.Mode.Equals("CustomerCloud", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Uri.TryCreate(request.Endpoint, UriKind.Absolute, out var endpoint) || endpoint.Scheme != Uri.UriSchemeHttps ||
                        !string.IsNullOrEmpty(endpoint.UserInfo) || string.IsNullOrWhiteSpace(request.Model) ||
                        string.IsNullOrWhiteSpace(request.ApiKey) && !request.KeepExistingApiKey)
                        return Results.BadRequest("Provide an HTTPS model endpoint, model name, and API key.");
                    var existingCloudPolicy = await db.AiTenantPolicies.SingleOrDefaultAsync(x => x.TenantId == request.TenantId, token);
                    if (existingCloudPolicy is not null && !string.IsNullOrWhiteSpace(existingCloudPolicy.ApiKeyReference))
                        return Results.Conflict("This tenant uses an administrator-managed key reference. Ask an administrator to change the AI policy.");
                    var savedCloud = await policies.SaveAsync(request with { ApiKeyReference = null }, token);
                    return Results.Ok(new { savedCloud.TenantId, savedCloud.Mode, savedCloud.Provider, savedCloud.Endpoint, savedCloud.Model,
                        hasApiKey = !string.IsNullOrWhiteSpace(savedCloud.ApiKeyReference) || !string.IsNullOrWhiteSpace(savedCloud.ApiKey), savedCloud.MonthlyTokenLimit,
                        savedCloud.MonthlyRequestLimit, savedCloud.MaxOutputTokens });
                }
                var existing = await db.AiTenantPolicies.SingleOrDefaultAsync(x => x.TenantId == request.TenantId, token);
                if (existing is null)
                {
                    existing = new BuilderAiTenantPolicy { TenantId = request.TenantId };
                    db.AiTenantPolicies.Add(existing);
                }
                existing.Mode = request.Mode;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                existing.Version++;
                await db.SaveChangesAsync(token);
                return Results.Ok(new { existing.TenantId, existing.Mode, existing.MonthlyTokenLimit, existing.MonthlyRequestLimit, existing.MaxOutputTokens });
            }
            var saved = await policies.SaveAsync(request, token);
            return Results.Ok(new { saved.TenantId, saved.Mode, saved.Provider, saved.Endpoint, saved.Model,
                hasApiKey = !string.IsNullOrWhiteSpace(saved.ApiKeyReference), saved.MonthlyTokenLimit,
                saved.MonthlyRequestLimit, saved.MaxOutputTokens });
        });

        group.MapGet("/usage", async (Guid? tenantId, DateOnly? month, ClaimsPrincipal user,
            BuilderDbContext db, CancellationToken token) =>
        {
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && !user.IsInRole(BuilderRoleNames.TenantAdmin))
                return Results.Forbid();
            if (tenantId is null && !user.IsInRole(BuilderRoleNames.SystemAdmin)) return Results.BadRequest("Tenant ID is required.");
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin) && tenantId is { } requestedTenant && !OwnsTenant(user, requestedTenant))
                return Results.Forbid();

            var start = month is { } selectedMonth ? new DateOnly(selectedMonth.Year, selectedMonth.Month, 1)
                : new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var end = start.AddMonths(1);
            var query = db.AiUsage.AsNoTracking().Where(x => x.CreatedAt >= start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) &&
                x.CreatedAt < end.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            if (tenantId is { } selectedTenant) query = query.Where(x => x.TenantId == selectedTenant);

            var totalRequests = await query.CountAsync(token);
            var inputTokens = await query.SumAsync(x => (long?)x.InputTokens, token) ?? 0;
            var outputTokens = await query.SumAsync(x => (long?)x.OutputTokens, token) ?? 0;
            var rows = await query.OrderByDescending(x => x.CreatedAt).Take(500)
                .Select(x => new { x.TenantId, x.ApplicationId, x.UserId, x.Feature, x.Provider, x.Model,
                    x.InputTokens, x.OutputTokens, x.IsEstimated, x.Outcome, x.FailureCode,
                    x.DurationMilliseconds, x.CreatedAt })
                .ToListAsync(token);
            return Results.Ok(new { month = start, totalRequests, inputTokens, outputTokens, totalTokens = inputTokens + outputTokens, items = rows });
        });
    }

    private static bool OwnsTenant(ClaimsPrincipal user, Guid tenantId)
    {
        var value = user.FindFirstValue("tenant_id") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId");
        return Guid.TryParse(value, out var claimedTenant) && claimedTenant == tenantId;
    }

    private static bool IsPublicEndpointHost(string host)
    {
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) || host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase)) return false;
        return !System.Net.IPAddress.TryParse(host, out var address) || AiInferenceService.IsPublicAddress(address);
    }
}

public sealed record SavePersonalAiModelRequest(Guid TenantId, string Provider, string? Endpoint, string Model,
    string? ApiKey, string? AuthScheme);
