using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SorteFederation.PaymentGateway.Abstractions;

namespace SorteFederation.PaymentGateway.Interfaces;

/// <summary>Fluent platform setup. Provider packages add their own extension (AddStripe, AddPayPal, ...).</summary>
public sealed class PaymentPlatformBuilder
{
    internal PaymentPlatformBuilder(IServiceCollection services) => Services = services;

    public IServiceCollection Services { get; }
    internal RoutingOptions Routing { get; } = new();

    public PaymentPlatformBuilder AddProvider<T>(PaymentProviderType type) where T : class, IPaymentProvider
    {
        Services.AddKeyedScoped<IPaymentProvider, T>(type);
        return this;
    }

    public PaymentPlatformBuilder UseTenantConfigStore<T>() where T : class, ITenantPaymentConfigStore
    { Services.AddScoped<ITenantPaymentConfigStore, T>(); return this; }

    public PaymentPlatformBuilder WithRepository<T>() where T : class, IPaymentRepository
    { Services.AddScoped<IPaymentRepository, T>(); return this; }

    public PaymentPlatformBuilder WithIdempotencyStore<T>() where T : class, IIdempotencyStore
    { Services.AddSingleton<IIdempotencyStore, T>(); return this; }

    public PaymentPlatformBuilder WithDefaultProvider(PaymentProviderType type)
    { Routing.Default = type; return this; }

    public PaymentPlatformBuilder WithFallback(params PaymentProviderType[] providers)
    { Routing.Fallbacks = providers; return this; }

    public PaymentPlatformBuilder WithRouting(Action<RoutingBuilder> configure)
    { configure(new RoutingBuilder(Routing)); return this; }
}

public static class PaymentServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentPlatform(this IServiceCollection services, Action<PaymentPlatformBuilder> configure)
    {
        var builder = new PaymentPlatformBuilder(services);
        configure(builder);

        services.AddSingleton(builder.Routing);
        services.AddScoped<IProviderResolver, ProviderResolver>();
        services.AddScoped<IPaymentService, PaymentService>();

        // Dev defaults, only if the caller did not register their own.
        services.TryAddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
        services.TryAddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        return services;
    }
}
