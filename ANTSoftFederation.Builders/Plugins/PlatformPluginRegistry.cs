namespace SorteFederation.Builders.Plugins;

public sealed record PlatformPluginDescriptor(string Id, string Version);

public sealed class PlatformPluginRegistry
{
    private readonly Dictionary<string, PlatformPluginDescriptor> plugins = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<PlatformPluginDescriptor> Plugins => plugins.Values;

    public void Register(IPlatformPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        if (string.IsNullOrWhiteSpace(plugin.Id)) throw new ArgumentException("Plugin id is required.", nameof(plugin));
        if (string.IsNullOrWhiteSpace(plugin.Version)) throw new ArgumentException("Plugin version is required.", nameof(plugin));
        if (!plugins.TryAdd(plugin.Id, new(plugin.Id, plugin.Version)))
            throw new InvalidOperationException($"A platform plugin with id '{plugin.Id}' is already registered.");
    }
}
