using Microsoft.Extensions.DependencyInjection;
using SorteFederation.PaymentGateway.Abstractions;

namespace SorteFederation.PaymentGateway.Interfaces;

/// <summary>Order: explicit request → client preference → routing rules → platform default → fallbacks.
/// Only providers enabled for the tenant survive.</summary>
public sealed class ProviderResolver(ITenantPaymentConfigStore tenants, RoutingOptions routing, IServiceProvider sp) : IProviderResolver
{
    public async Task<IReadOnlyList<ResolvedProvider>> ResolveCandidatesAsync(PaymentRequest req, CancellationToken ct)
    {
        var cfg = await tenants.GetAsync(req.TenantId, ct);
        List<PaymentProviderType> order;

        if (req.Provider is { } explicitType)
        {
            if (!cfg.IsEnabled(explicitType)) throw new ProviderNotEnabledException(explicitType, req.TenantId);
            order = [explicitType];
        }
        else
        {
            var ctx = new RoutingContext(req.TenantId, req.Amount.Currency, req.Customer.Country, req.Method);
            order = new PaymentProviderType?[] { cfg.PreferredFor(req.Amount.Currency), routing.Match(ctx), routing.Default }
                .Concat(routing.Fallbacks.Select(f => (PaymentProviderType?)f))
                .Where(t => t.HasValue).Select(t => t!.Value)
                .Distinct()
                .Where(cfg.IsEnabled)
                .ToList();
        }

        if (order.Count == 0) throw new ProviderNotEnabledException(null, req.TenantId);
        return order.Select(t => new ResolvedProvider(sp.GetRequiredKeyedService<IPaymentProvider>(t), cfg.CredentialsFor(t))).ToList();
    }

    public async Task<ResolvedProvider> ResolveForTenantAsync(PaymentProviderType type, string tenantId, CancellationToken ct)
    {
        var cfg = await tenants.GetAsync(tenantId, ct);
        if (!cfg.IsEnabled(type)) throw new ProviderNotEnabledException(type, tenantId);
        return new ResolvedProvider(sp.GetRequiredKeyedService<IPaymentProvider>(type), cfg.CredentialsFor(type));
    }
}
