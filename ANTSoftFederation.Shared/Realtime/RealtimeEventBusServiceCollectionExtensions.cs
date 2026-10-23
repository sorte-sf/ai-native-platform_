using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace SorteFederation.Shared.Realtime;

public static class RealtimeEventBusServiceCollectionExtensions
{
    public static IServiceCollection AddRealtimeEventBus(this IServiceCollection services, string subscriberName)
    {
        services.TryAddSingleton<IRealtimeEventBus, RabbitMqRealtimeEventBus>();
        services.AddSingleton<IHostedService>(provider => new RealtimeEventInbox(
            provider.GetRequiredService<IRealtimeEventBus>(),
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RealtimeEventInbox>>(),
            subscriberName));
        return services;
    }
}
