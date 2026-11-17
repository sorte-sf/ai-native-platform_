using SorteFederation.PaymentGateway.Abstractions;

namespace SorteFederation.PaymentGateway.Interfaces;

public sealed record RoutingContext(string TenantId, string Currency, string? Country, PaymentMethod Method);

public sealed class RoutingOptions
{
    internal List<(Func<RoutingContext, bool> When, PaymentProviderType Use)> Rules { get; } = [];
    public PaymentProviderType? Default { get; internal set; }
    public IReadOnlyList<PaymentProviderType> Fallbacks { get; internal set; } = [];

    public PaymentProviderType? Match(RoutingContext ctx)
    {
        foreach (var (when, use) in Rules) if (when(ctx)) return use;
        return null;
    }
}

public sealed class RoutingBuilder(RoutingOptions options)
{
    public RuleBuilder When(Func<RoutingContext, bool> predicate) => new(this, predicate);

    public sealed class RuleBuilder(RoutingBuilder parent, Func<RoutingContext, bool> predicate)
    {
        public RoutingBuilder Use(PaymentProviderType provider)
        {
            parent._options.Rules.Add((predicate, provider));
            return parent;
        }
    }
    private readonly RoutingOptions _options = options;
}
