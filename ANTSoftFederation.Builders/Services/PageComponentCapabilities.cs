namespace SorteFederation.Builders.Services;

/// <summary>Trusted build-time contract shared with the Angular page loader.</summary>
public static class PageComponentCapabilities
{
    private sealed record Component(string Type, bool Children, List<string> Properties, string? Binding);
    private sealed record Property(string Key, string Label, string Editor, int MaxLength);
    private sealed record Manifest(int SchemaVersion, List<Component> Components, List<Property> Properties);
    private static readonly Manifest Contract = Load();
    private static readonly Dictionary<string, bool> Types =
        Contract.Components.ToDictionary(component => component.Type, component => component.Children, StringComparer.Ordinal);
    public static int SchemaVersion => Contract.SchemaVersion;
    public static bool TryGet(string type, out bool children) => Types.TryGetValue(type, out children);
    private static readonly Dictionary<string, string?> Bindings =
        Contract.Components.ToDictionary(component => component.Type, component => component.Binding, StringComparer.Ordinal);
    public static string? BindingFor(string type) => Bindings.GetValueOrDefault(type);
    public static void ValidateProperties(PageNode node)
    {
        foreach (var property in Contract.Properties)
        {
            var value = property.Key switch { "text" => node.Text, "imageUrl" => node.ImageUrl, "pageKey" => node.PageKey, _ => null };
            if (value?.Length > property.MaxLength) throw new ArgumentException("Component property exceeds length limit.");
        }
    }

    private static Manifest Load()
    {
        using var stream = typeof(PageComponentCapabilities).Assembly.GetManifestResourceStream("PageComponents.v1.json")
            ?? throw new InvalidOperationException("Component capability manifest is missing.");
        var manifest = JsonSerializer.Deserialize<Manifest>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Invalid component capability manifest.");
        if (manifest.SchemaVersion != 1 || manifest.Components is null || manifest.Components.Count == 0
            || manifest.Components.Any(component => component is null || string.IsNullOrWhiteSpace(component.Type))
            || manifest.Components.Select(component => component.Type).Distinct(StringComparer.Ordinal).Count() != manifest.Components.Count)
            throw new InvalidOperationException("Invalid component capability manifest.");
        if (manifest.Properties is null || manifest.Properties.Count == 0
            || manifest.Properties.Any(property => property is null || property.Key is not ("text" or "imageUrl" or "pageKey")
                || property.Editor is not ("multiline" or "url" or "page") || property.MaxLength < 1 || string.IsNullOrWhiteSpace(property.Label))
            || manifest.Properties.Select(property => property.Key).Distinct(StringComparer.Ordinal).Count() != manifest.Properties.Count
            || manifest.Components.Any(component => component.Binding is not (null or "form" or "object" or "object-or-json") || component.Properties is null
                || component.Properties.Any(key => !manifest.Properties.Any(property => property.Key == key))))
            throw new InvalidOperationException("Invalid component property contract.");
        return manifest;
    }
}
