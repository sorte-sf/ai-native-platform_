using Microsoft.Extensions.Options;
using SorteFederation.PaymentGateway.Abstractions;

namespace SorteFederation.PaymentGateway.Interfaces;

public sealed class ProviderSetting
{
    public bool Enabled { get; set; } = true;
    public Dictionary<string, string> Credentials { get; set; } = [];
}

public sealed class TenantPaymentConfig
{
    public PaymentProviderType? Preferred { get; set; }
    public Dictionary<string, PaymentProviderType> PreferredByCurrency { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<PaymentProviderType, ProviderSetting> Providers { get; set; } = [];

    public PaymentProviderType? PreferredFor(string currency) =>
        PreferredByCurrency.TryGetValue(currency, out var p) ? p : Preferred;
    public bool IsEnabled(PaymentProviderType t) => Providers.TryGetValue(t, out var s) && s.Enabled;
    public ProviderCredentials CredentialsFor(PaymentProviderType t) =>
        Providers.TryGetValue(t, out var s) ? new ProviderCredentials(s.Credentials)
            : throw new PaymentConfigurationException($"No credentials for {t}.");
}

public interface ITenantPaymentConfigStore
{
    Task<TenantPaymentConfig> GetAsync(string tenantId, CancellationToken ct);
}

public sealed class PaymentTenantsOptions
{
    public Dictionary<string, TenantPaymentConfig> Tenants { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>Reads tenants from configuration with live reload (IOptionsMonitor). Swap for an EF/Key Vault store in production.</summary>
public sealed class ConfigurationTenantConfigStore(IOptionsMonitor<PaymentTenantsOptions> options) : ITenantPaymentConfigStore
{
    public Task<TenantPaymentConfig> GetAsync(string tenantId, CancellationToken ct) =>
        options.CurrentValue.Tenants.TryGetValue(tenantId, out var cfg)
            ? Task.FromResult(cfg)
            : throw new PaymentConfigurationException($"Unknown tenant '{tenantId}'.");
}
