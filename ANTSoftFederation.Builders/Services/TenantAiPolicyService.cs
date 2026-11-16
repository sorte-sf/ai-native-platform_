using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Services;

public sealed record ResolvedAiPolicy(
    Guid TenantId,
    string Mode,
    string Provider,
    string? Endpoint,
    string? Model,
    string? ApiKeyReference,
    string AuthScheme,
    long MonthlyTokenLimit,
    int MonthlyRequestLimit,
    int MaxOutputTokens);

public sealed class TenantAiPolicyService(BuilderDbContext db, IConfiguration configuration)
{
    public async Task<ResolvedAiPolicy> ResolveAsync(Guid tenantId, string feature, CancellationToken token)
    {
        var stored = await db.AiTenantPolicies.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId, token);
        if (stored is not null)
        {
            var platformManaged = stored.Mode.Equals("PlatformManaged", StringComparison.OrdinalIgnoreCase);
            return new ResolvedAiPolicy(tenantId, stored.Mode,
                string.IsNullOrWhiteSpace(stored.Provider) && platformManaged ? configuration["AI:Provider"] ?? "openai-compatible" : stored.Provider,
                stored.Endpoint ?? (platformManaged ? configuration["AI:Endpoint"] : null),
                stored.Model ?? (platformManaged ? configuration["AI:Model"] : null),
                stored.ApiKeyReference ?? (platformManaged ? configuration["AI:ApiKeyReference"] : null),
                platformManaged ? configuration["AI:AuthScheme"] ?? stored.AuthScheme : stored.AuthScheme,
                stored.MonthlyTokenLimit, stored.MonthlyRequestLimit, stored.MaxOutputTokens);
        }

        var globalProvider = configuration["AI:Provider"] ?? "disabled";
        var localDesignerDefault = feature.Equals("page-designer", StringComparison.OrdinalIgnoreCase) ||
                                  feature.Equals("application-scaffold", StringComparison.OrdinalIgnoreCase);
        var mode = globalProvider.Equals("disabled", StringComparison.OrdinalIgnoreCase)
            ? localDesignerDefault ? "Local" : "Disabled"
            : globalProvider.Equals("local", StringComparison.OrdinalIgnoreCase) || globalProvider.Equals("ollama", StringComparison.OrdinalIgnoreCase)
                ? "Local" : "PlatformManaged";
        return new ResolvedAiPolicy(tenantId, mode, globalProvider,
            configuration["AI:Endpoint"], configuration["AI:Model"], configuration["AI:ApiKeyReference"],
            configuration["AI:AuthScheme"] ?? "bearer", 100_000, 1_000, 4_096);
    }

    public async Task<ResolvedAiPolicy> GetAsync(Guid tenantId, CancellationToken token) =>
        await ResolveAsync(tenantId, "platform", token);

    public async Task<ResolvedAiPolicy> SaveAsync(UpdateTenantAiPolicyRequest request, CancellationToken token)
    {
        if (request.TenantId == Guid.Empty) throw new ArgumentException("A tenant is required.");
        if (!new[] { "Disabled", "Local", "CustomerCloud", "PlatformManaged" }.Contains(request.Mode, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("AI mode must be Disabled, Local, CustomerCloud, or PlatformManaged.");
        if (request.MonthlyTokenLimit < 0 || request.MonthlyRequestLimit < 0 || request.MaxOutputTokens is < 1 or > 32_000)
            throw new ArgumentException("AI usage limits are outside the supported range.");
        if (request.Mode.Equals("CustomerCloud", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(request.Endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
                !string.IsNullOrEmpty(uri.UserInfo) || string.IsNullOrWhiteSpace(request.Model))
                throw new ArgumentException("Cloud AI requires an HTTPS endpoint and model name.");
            if (string.IsNullOrWhiteSpace(request.ApiKeyReference))
                throw new ArgumentException("Customer-managed cloud AI requires a secure API-key reference.");
        }
        if (request.Mode.Equals("PlatformManaged", StringComparison.OrdinalIgnoreCase) &&
            (!Uri.TryCreate(request.Endpoint ?? configuration["AI:Endpoint"], UriKind.Absolute, out var platformUri) ||
             platformUri.Scheme != Uri.UriSchemeHttps || string.IsNullOrWhiteSpace(request.Model ?? configuration["AI:Model"])))
            throw new ArgumentException("Platform-managed AI must have a configured HTTPS endpoint and model.");

        var policy = await db.AiTenantPolicies.SingleOrDefaultAsync(x => x.TenantId == request.TenantId, token);
        if (policy is null)
        {
            policy = new BuilderAiTenantPolicy { TenantId = request.TenantId };
            db.AiTenantPolicies.Add(policy);
        }
        policy.Mode = request.Mode;
        policy.Provider = request.Provider?.Trim() ?? "";
        policy.Endpoint = request.Endpoint?.Trim();
        policy.Model = request.Model?.Trim();
        policy.ApiKeyReference = request.ApiKeyReference?.Trim();
        policy.AuthScheme = request.AuthScheme?.Trim().ToLowerInvariant() is "api-key" ? "api-key" : "bearer";
        policy.MonthlyTokenLimit = request.MonthlyTokenLimit;
        policy.MonthlyRequestLimit = request.MonthlyRequestLimit;
        policy.MaxOutputTokens = request.MaxOutputTokens;
        policy.UpdatedAt = DateTimeOffset.UtcNow;
        policy.Version++;
        await db.SaveChangesAsync(token);
        return new ResolvedAiPolicy(policy.TenantId, policy.Mode, policy.Provider, policy.Endpoint, policy.Model,
            policy.ApiKeyReference, policy.AuthScheme, policy.MonthlyTokenLimit, policy.MonthlyRequestLimit, policy.MaxOutputTokens);
    }
}

public sealed record UpdateTenantAiPolicyRequest(
    Guid TenantId,
    string Mode,
    string? Provider,
    string? Endpoint,
    string? Model,
    string? ApiKeyReference,
    string? AuthScheme,
    long MonthlyTokenLimit,
    int MonthlyRequestLimit,
    int MaxOutputTokens);
