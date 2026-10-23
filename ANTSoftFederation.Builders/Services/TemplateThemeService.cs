using System.Text.Json;
using System.Text.RegularExpressions;

namespace SorteFederation.Builders.Services;

public static partial class TemplateThemeService
{
    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex ColorPattern();

    public static string Validate(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object) throw new ArgumentException("Theme must be an object.");
        var allowed = new HashSet<string>(StringComparer.Ordinal) { "primaryColor", "backgroundColor", "surfaceColor", "textColor", "logoUrl", "fontScale", "fontFamily", "fontSize", "shell" };
        foreach (var item in root.EnumerateObject())
        {
            if (!allowed.Contains(item.Name)) throw new ArgumentException($"Unknown theme property '{item.Name}'.");
            if (item.Name.EndsWith("Color", StringComparison.Ordinal))
            {
                if (item.Value.ValueKind != JsonValueKind.String || !ColorPattern().IsMatch(item.Value.GetString() ?? ""))
                    throw new ArgumentException($"{item.Name} must be a six-digit hex color.");
            }
            else if (item.Name == "fontScale")
            {
                if (item.Value.ValueKind != JsonValueKind.Number || !item.Value.TryGetDouble(out var scale) || scale is < 0.8 or > 1.4)
                    throw new ArgumentException("fontScale must be between 0.8 and 1.4.");
            }
            else if (item.Name == "fontSize")
            {
                if (item.Value.ValueKind != JsonValueKind.Number || !item.Value.TryGetDouble(out var size) || size is < 10 or > 32)
                    throw new ArgumentException("fontSize must be between 10 and 32 pixels.");
            }
            else if (item.Name == "fontFamily")
            {
                if (item.Value.ValueKind != JsonValueKind.String || item.Value.GetString() is not
                    ("Inter, Arial, sans-serif" or "Arial, Helvetica, sans-serif" or "Verdana, Geneva, sans-serif" or
                     "Georgia, serif" or "Trebuchet MS, sans-serif" or "Courier New, monospace" or "system-ui, sans-serif" or "monospace"))
                    throw new ArgumentException("fontFamily must be a supported font stack.");
            }
            else if (item.Name == "shell") ValidateShell(item.Value);
            else if (item.Value.ValueKind != JsonValueKind.String ||
                item.Value.GetString() is not { Length: <= 500 } url ||
                !(url.Length == 0 || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("/assets/", StringComparison.Ordinal)))
                throw new ArgumentException("logoUrl must be an HTTPS URL or a /assets/ path.");
        }
        return JsonSerializer.Serialize(root);
    }

    private static void ValidateShell(JsonElement shell)
    {
        if (shell.ValueKind != JsonValueKind.Object) throw new ArgumentException("shell must be an object.");
        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "brand", "logoUrl", "profileName", "profileRole", "language", "showSearch", "showMegaMenu",
            "showNotifications", "showSettings", "showThemeToggle", "themeMode", "sidebarCollapsed",
            "sidebarPosition", "sidebarWidth", "headerPosition", "headerHeight", "sidebarBackground",
            "sidebarTextColor", "headerBackground", "headerTextColor"
        };
        foreach (var property in shell.EnumerateObject())
        {
            if (!allowed.Contains(property.Name)) throw new ArgumentException($"Unknown shell property '{property.Name}'.");
            if (property.Name is "brand" or "profileName" or "profileRole" or "language")
            {
                if (property.Value.ValueKind != JsonValueKind.String || property.Value.GetString() is not { Length: <= 120 })
                    throw new ArgumentException($"shell.{property.Name} must be a short string.");
            }
            else if (property.Name == "logoUrl")
            {
                if (property.Value.ValueKind != JsonValueKind.String || property.Value.GetString() is not { Length: <= 500 } url ||
                    !(url.Length == 0 || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("/assets/", StringComparison.Ordinal)))
                    throw new ArgumentException("shell.logoUrl must be an HTTPS URL or /assets/ path.");
            }
            else if (property.Name is "themeMode" or "sidebarPosition" or "headerPosition")
            {
                var value = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
                var valid = property.Name switch
                {
                    "themeMode" => value is "light" or "dark",
                    "sidebarPosition" => value is "left" or "right",
                    _ => value is "top" or "bottom"
                };
                if (!valid) throw new ArgumentException($"shell.{property.Name} has an unsupported value.");
            }
            else if (property.Name is "sidebarWidth" or "headerHeight")
            {
                if (property.Value.ValueKind != JsonValueKind.Number || !property.Value.TryGetInt32(out var number) ||
                    (property.Name == "sidebarWidth" ? number is < 64 or > 480 : number is < 40 or > 160))
                    throw new ArgumentException($"shell.{property.Name} is outside the supported range.");
            }
            else if (property.Name is "sidebarBackground" or "sidebarTextColor" or "headerBackground" or "headerTextColor")
            {
                if (property.Value.ValueKind != JsonValueKind.String || !ColorPattern().IsMatch(property.Value.GetString() ?? ""))
                    throw new ArgumentException($"shell.{property.Name} must be a six-digit hex color.");
            }
            else if (property.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                throw new ArgumentException($"shell.{property.Name} must be a boolean.");
        }
    }
}
