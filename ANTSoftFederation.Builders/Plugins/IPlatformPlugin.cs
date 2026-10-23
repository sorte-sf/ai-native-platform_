using Microsoft.Extensions.DependencyInjection;

namespace SorteFederation.Builders.Plugins;

/// <summary>
/// Contract for in-process Builders extensions. Plugins extend capabilities;
/// they do not access another plugin's persistence tables directly.
/// </summary>
public interface IPlatformPlugin
{
    string Id { get; }
    string Version { get; }

    void Register(PlatformPluginContext context);
}

public sealed class PlatformPluginContext(IServiceCollection services, PlatformPluginRegistry registry)
{
    public IServiceCollection Services { get; } = services;
    public PlatformPluginRegistry Registry { get; } = registry;
}
