using System.IO.Compression;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SorteFederation.DesignerStudio.Storage;

/// <summary>Translates a saved Design Studio draft into the shared Builder template manifest.</summary>
public static class DraftTemplatePackageGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex KeyPattern = new("^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public static byte[] Generate(string templateKey, string templateName, string version, string draftJson)
    {
        using var draft = JsonDocument.Parse(draftJson);
        var root = draft.RootElement;
        if (root.ValueKind != JsonValueKind.Object || !GetInteger(root, "formatVersion", 0).Equals(1))
            throw new ArgumentException("The saved design draft has an unsupported format.");

        var pages = ReadPages(root, templateName);
        var models = ReadModels(root);
        var modelKeys = models.Select(model => model.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var fieldsByModel = models.ToDictionary(model => model.Key,
            model => model.Fields.Select(field => field.Key).ToHashSet(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        var bindings = new List<object>();
        var shell = root.TryGetProperty("shell", out var shellElement) ? shellElement : default;
        var theme = root.TryGetProperty("theme", out var themeElement) ? themeElement : default;
        var layout = root.TryGetProperty("layout", out var layoutElement) ? layoutElement : default;
        var requestedColumns = GetInteger(layout, "columns", 3);
        var pageColumns = requestedColumns is 2 or 3 or 4 or 5 or 6 or 12 ? requestedColumns : 3;
        var desktopColumns = Enumerable.Repeat(1d, pageColumns).ToArray();
        var tabletColumns = Enumerable.Repeat(1d, Math.Min(pageColumns, 2)).ToArray();

        var screens = pages.Select(page => BuildScreen(page, theme, desktopColumns, tabletColumns,
            models, modelKeys, fieldsByModel, bindings)).ToArray();
        var navigation = ReadNavigation(shell);
        var manifest = new
        {
            formatVersion = 1,
            template = new { key = templateKey, name = templateName, version },
            dataModels = models.Select(model => new { key = model.Key, name = model.Name, fields = model.Fields.Select(field => new { key = field.Key, type = field.Type }) }),
            bindings,
            version = 1,
            objects = models.Select(model => new
            {
                key = model.Key, name = model.Name,
                fields = model.Fields.Select(field => new { key = field.Key, label = field.Label, type = field.Type, required = false, searchable = field.Type is "Text" or "Email" }),
                forms = Array.Empty<object>()
            }),
            screens,
            navigation,
            shell = ReadShell(shell),
            theme = ReadTheme(theme)
        };

        var manifestJson = JsonSerializer.Serialize(manifest, JsonOptions);
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("manifest.json", CompressionLevel.Optimal);
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            writer.Write(manifestJson);
        }
        return output.ToArray();
    }

    private static object BuildScreen(DraftPage page, JsonElement theme,
        double[] desktopColumns, double[] tabletColumns, List<DraftModel> models, HashSet<string> modelKeys,
        Dictionary<string, HashSet<string>> fieldsByModel, List<object> bindings)
    {
        var pageBackground = GetString(theme, "pageBackground") ?? "#ffffff";
        var pageTextColor = GetString(theme, "textColor") ?? "#253044";
        var fontFamily = GetString(theme, "fontFamily") ?? "Inter, Arial, sans-serif";
        var children = new List<object>();
        var figmaChildren = new List<object>();
        var figmaFrame = page.Elements.FirstOrDefault(element =>
        {
            var style = GetProperty(element, "figmaStyle");
            return GetNumber(style, "frameWidth") > 0 && GetNumber(style, "frameHeight") > 0;
        });
        if (figmaFrame.ValueKind == JsonValueKind.Undefined)
            figmaFrame = page.Elements.FirstOrDefault(element =>
            {
                var style = GetProperty(element, "figmaStyle");
                return GetBoolean(style, "imported", false) &&
                    string.Equals(GetString(element, "label"), page.Title, StringComparison.OrdinalIgnoreCase) &&
                    GetNumber(style, "width") > 0 && GetNumber(style, "height") > 0;
            });
        var frameStyle = figmaFrame.ValueKind == JsonValueKind.Undefined ? default : GetProperty(figmaFrame, "figmaStyle");
        var frameWidth = GetNumber(frameStyle, "frameWidth");
        var frameHeight = GetNumber(frameStyle, "frameHeight");
        if (frameWidth <= 0) frameWidth = GetNumber(frameStyle, "width");
        if (frameHeight <= 0) frameHeight = GetNumber(frameStyle, "height");
        var frameId = figmaFrame.ValueKind == JsonValueKind.Object ? GetString(figmaFrame, "id") : null;
        foreach (var element in page.Elements)
        {
            var id = GetString(element, "id") ?? $"component-{children.Count + 1}";
            if (id == "app-shell") continue;
            if (id == frameId) continue;
            var label = GetString(element, "label") ?? "Component";
            var kind = GetString(element, "kind") ?? "Text";
            var modelKey = GetString(element, "modelKey");
            var fieldKey = GetString(element, "fieldKey");
            if (modelKey is not null && !modelKeys.Contains(modelKey))
                throw new ArgumentException($"Component '{label}' references an unknown data model.");
            if (modelKey is not null && fieldKey is not null)
            {
                if (!fieldsByModel[modelKey].Contains(fieldKey)) throw new ArgumentException($"Component '{label}' references an unknown model field.");
                bindings.Add(new { modelKey, fieldKey });
            }
            var chartValueFieldKey = GetString(element, "chartValueFieldKey");
            var chartSecondaryValueFieldKey = GetString(element, "chartSecondaryValueFieldKey");
            foreach (var chartField in new[] { chartValueFieldKey, chartSecondaryValueFieldKey }.Where(key => key is not null))
            {
                if (modelKey is null || !fieldsByModel[modelKey].Contains(chartField!))
                    throw new ArgumentException($"Chart '{label}' references a field outside its selected data model.");
                var valueType = models.Single(model => model.Key.Equals(modelKey, StringComparison.OrdinalIgnoreCase))
                    .Fields.Single(field => field.Key.Equals(chartField, StringComparison.OrdinalIgnoreCase)).Type;
                if (valueType is not ("Integer" or "Decimal"))
                    throw new ArgumentException($"Chart '{label}' value fields must use a numeric data type.");
            }

            var type = kind switch
            {
                "Heading" => "heading", "Button" => "button", "Group" or "Horizontal List" => "container",
                "Icon" => "icon", "Image" => "image", "Chart" => "chart", "Calendar" => "calendar",
                "KPI" => "kpi", "Data table" or "Repeating Grid" or "Table" => "data-grid", _ => "text"
            };
            var node = new Dictionary<string, object?>
            {
                ["id"] = id, ["type"] = type,
                ["text"] = GetString(element, "text") ?? label
            };
            var figmaStyle = ReadFigmaStyle(element);
            var isImportedFigmaLayer = GetBoolean(GetProperty(element, "figmaStyle"), "imported", false);
            if (isImportedFigmaLayer && frameWidth > 0 && frameHeight > 0)
            {
                AddFigmaPosition(figmaStyle, GetProperty(element, "figmaStyle"), frameWidth, frameHeight);
                AddFigmaFontScaling(figmaStyle, GetProperty(element, "figmaStyle"), frameWidth);
            }
            if (figmaStyle.Count > 0) node["styling"] = figmaStyle;
            if (type == "chart")
            {
                var requestedChartKind = GetString(element, "chartType");
                node["chartKind"] = requestedChartKind is "bar" or "line" or "area" or "stacked-column" or "pie" or "doughnut"
                    ? requestedChartKind
                    : "bar";
                if (requestedChartKind == "stacked-column" && chartSecondaryValueFieldKey is not null && chartSecondaryValueFieldKey == chartValueFieldKey)
                    throw new ArgumentException($"Chart '{label}' must use different fields for its stacked values.");
                if (modelKey is not null && fieldKey is not null) node["columns"] = new[] { fieldKey };
                if (chartValueFieldKey is not null) node["valueField"] = chartValueFieldKey;
                if (chartSecondaryValueFieldKey is not null) node["secondaryValueField"] = chartSecondaryValueFieldKey;
            }
            if (modelKey is not null) node["objectKey"] = modelKey;
            if (GetString(element, "imageUrl") is { } imageUrl && type == "image") node["imageUrl"] = imageUrl;
            if (type == "data-grid")
            {
                var source = modelKey ?? models[0].Key;
                var model = models.Single(item => item.Key.Equals(source, StringComparison.OrdinalIgnoreCase));
                node["objectKey"] = source;
                node["columns"] = model.Fields.Take(50).Select(field => field.Key).ToArray();
                node["tableColumns"] = model.Fields.Take(50).Select(field => new { field = field.Key, title = field.Label, align = "left", format = "text" }).ToArray();
            }
            (isImportedFigmaLayer && frameWidth > 0 && frameHeight > 0 ? figmaChildren : children).Add(node);
        }

        if (figmaChildren.Count > 0 && frameWidth > 0 && frameHeight > 0)
        {
            var artboardStyle = new Dictionary<string, string>
            {
                ["display"] = "block",
                ["position"] = "relative",
                ["container-type"] = "inline-size",
                ["width"] = "100%",
                ["max-width"] = $"{frameWidth.ToString("0.##", CultureInfo.InvariantCulture)}px",
                ["aspect-ratio"] = $"{frameWidth.ToString("0.##", CultureInfo.InvariantCulture)} / {frameHeight.ToString("0.##", CultureInfo.InvariantCulture)}",
                ["background-color"] = GetString(frameStyle, "backgroundColor") ?? pageBackground
            };
            children.Insert(0, new Dictionary<string, object?>
            {
                ["id"] = $"figma-artboard-{page.Key}",
                ["type"] = "container",
                ["text"] = "",
                ["styling"] = artboardStyle,
                ["children"] = figmaChildren
            });
        }

        var content = new Dictionary<string, object?>
        {
            ["id"] = $"page-content-{page.Key}",
            ["type"] = "container",
            ["text"] = "",
            ["styling"] = new Dictionary<string, string>
            {
                ["display"] = "grid", ["gap"] = "16px", ["min-height"] = "100%",
                ["background-color"] = pageBackground, ["color"] = pageTextColor, ["font-family"] = fontFamily
            },
            ["responsiveColumns"] = new { desktop = desktopColumns, tablet = tabletColumns, mobile = new[] { 1d } },
            ["children"] = children
        };
        var definition = JsonSerializer.Serialize(new
        {
            schemaVersion = 1, pageType = page.Type,
            page = new { title = page.Title, description = page.Description },
            children = new[] { content }
        }, JsonOptions);
        return new { key = page.Key, title = page.Title, objectKey = models[0].Key, htmlContent = "", definitionJson = definition };
    }

    private static object ReadShell(JsonElement shell) => new
    {
        brand = GetString(shell, "brand") ?? "",
        logoUrl = GetString(shell, "logoUrl") ?? "",
        profileName = GetString(shell, "profileName") ?? "User",
        profileRole = GetString(shell, "profileRole") ?? "Member",
        language = GetString(shell, "language") ?? "EN",
        showSearch = GetBoolean(shell, "showSearch", true),
        showMegaMenu = GetBoolean(shell, "showMegaMenu", true),
        showNotifications = GetBoolean(shell, "showNotifications", true),
        showSettings = GetBoolean(shell, "showSettings", true),
        showThemeToggle = GetBoolean(shell, "showThemeToggle", true),
        themeMode = GetString(shell, "themeMode") is "dark" ? "dark" : "light",
        sidebarCollapsed = GetBoolean(shell, "sidebarCollapsed", false),
        sidebarPosition = GetString(shell, "sidebarPosition") is "right" ? "right" : "left",
        sidebarWidth = Math.Clamp(GetInteger(shell, "sidebarWidth", 240), 64, 480),
        headerPosition = GetString(shell, "headerPosition") is "bottom" ? "bottom" : "top",
        headerHeight = Math.Clamp(GetInteger(shell, "headerHeight", 64), 40, 160),
        sidebarBackground = SafeColor(GetString(shell, "sidebarBackground")) ?? (GetString(shell, "themeMode") == "dark" ? "#22252d" : "#ffffff"),
        sidebarTextColor = SafeColor(GetString(shell, "sidebarTextColor")) ?? (GetString(shell, "themeMode") == "dark" ? "#f2edf5" : "#253044"),
        headerBackground = SafeColor(GetString(shell, "headerBackground")) ?? (GetString(shell, "themeMode") == "dark" ? "#22252d" : "#ffffff"),
        headerTextColor = SafeColor(GetString(shell, "headerTextColor")) ?? (GetString(shell, "themeMode") == "dark" ? "#f2edf5" : "#253044")
    };

    private static object ReadTheme(JsonElement theme) => new
    {
        accentColor = SafeColor(GetString(theme, "accentColor")) ?? "#5e4fa2",
        pageBackground = SafeColor(GetString(theme, "pageBackground")) ?? "#ffffff",
        textColor = SafeColor(GetString(theme, "textColor")) ?? "#253044",
        fontFamily = GetString(theme, "fontFamily") is { Length: <= 80 } font ? font : "Inter, Arial, sans-serif",
        fontSize = theme.ValueKind == JsonValueKind.Object && theme.TryGetProperty("fontSize", out var size) && size.TryGetDouble(out var value) && value is >= 10 and <= 32 ? value : 16
    };

    private static string? SafeColor(string? value) => value is not null && Regex.IsMatch(value, "^#[0-9a-fA-F]{6}$") ? value : null;

    private static List<DraftPage> ReadPages(JsonElement root, string templateName)
    {
        var pages = new List<DraftPage>();
        if (root.TryGetProperty("pages", out var pageArray) && pageArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var page in pageArray.EnumerateArray())
            {
                if (page.ValueKind != JsonValueKind.Object) throw new ArgumentException("Each template page must be an object.");
                var key = GetString(page, "key") ?? "";
                var title = GetString(page, "title") ?? "Untitled page";
                var type = GetString(page, "type") ?? "page";
                var elements = page.TryGetProperty("elements", out var items) && items.ValueKind == JsonValueKind.Array
                    ? items.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object).Select(item => item.Clone()).ToList()
                    : [];
                pages.Add(new DraftPage(key, title, GetString(page, "description") ?? "", type, elements));
            }
        }
        else
        {
            var page = root.TryGetProperty("page", out var legacyPage) ? legacyPage : default;
            var elements = root.TryGetProperty("elements", out var legacyElements) && legacyElements.ValueKind == JsonValueKind.Array
                ? legacyElements.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object).Select(item => item.Clone()).ToList()
                : [];
            pages.Add(new DraftPage("home", GetString(page, "title") ?? templateName, GetString(page, "description") ?? "", "dashboard", elements));
        }
        if (pages.Count is < 1 or > 40) throw new ArgumentException("A template must contain between 1 and 40 pages.");
        if (pages.Any(page => !KeyPattern.IsMatch(page.Key) || string.IsNullOrWhiteSpace(page.Title) || page.Title.Length > 120 ||
            page.Description.Length > 2000 || page.Type is not ("dashboard" or "page" or "login" or "change-password")))
            throw new ArgumentException("A template page has an invalid key, name, type, or description.");
        if (pages.Select(page => page.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count() != pages.Count)
            throw new ArgumentException("Template page keys must be unique.");
        return pages;
    }

    private static List<object>? ReadNavigation(JsonElement shell)
    {
        if (shell.ValueKind != JsonValueKind.Object || !shell.TryGetProperty("navigation", out var items) || items.ValueKind != JsonValueKind.Array)
            return null;
        var nextSuffixByBaseKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        List<object> ReadItems(JsonElement array) => array.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object)
            .Select(item =>
            {
                var label = GetString(item, "label") ?? "Menu";
                var requestedKey = GetString(item, "key");
                var key = UniqueNavigationKey(requestedKey, label, nextSuffixByBaseKey);
                var children = item.TryGetProperty("children", out var nested) && nested.ValueKind == JsonValueKind.Array
                    ? ReadItems(nested)
                    : [];
                return (object)new
                {
                    key,
                    label,
                    icon = GetString(item, "icon"),
                    screenKey = GetString(item, "pageKey"),
                    children
                };
            }).ToList();
        return ReadItems(items);
    }

    private static string UniqueNavigationKey(string? requestedKey, string label,
        Dictionary<string, int> nextSuffixByBaseKey)
    {
        var baseKey = Slug(string.IsNullOrWhiteSpace(requestedKey) ? label : requestedKey);
        var suffix = nextSuffixByBaseKey.GetValueOrDefault(baseKey, 1);
        nextSuffixByBaseKey[baseKey] = suffix + 1;
        if (suffix == 1) return baseKey;
        var suffixText = $"-{suffix}";
        var key = baseKey[..Math.Min(baseKey.Length, 80 - suffixText.Length)] + suffixText;
        return key;
    }

    private static string Slug(string value)
    {
        var key = Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return KeyPattern.IsMatch(key) ? key : "menu";
    }

    private static List<DraftModel> ReadModels(JsonElement root)
    {
        var models = new List<DraftModel>();
        if (root.TryGetProperty("dataModels", out var modelArray) && modelArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var model in modelArray.EnumerateArray())
            {
                if (model.ValueKind != JsonValueKind.Object) throw new ArgumentException("Each data model in the saved design must be an object.");
                var key = GetString(model, "key") ?? "";
                var name = GetString(model, "name") ?? key;
                var fields = new List<DraftField>();
                if (model.TryGetProperty("fields", out var fieldArray) && fieldArray.ValueKind == JsonValueKind.Array)
                    foreach (var field in fieldArray.EnumerateArray())
                    {
                        if (field.ValueKind != JsonValueKind.Object) throw new ArgumentException($"Data model '{name}' contains an invalid field.");
                        var fieldKey = GetString(field, "key") ?? "";
                        var label = GetString(field, "label") ?? fieldKey;
                        var rawType = GetString(field, "type") ?? "Text";
                        var type = rawType.ToLowerInvariant() switch { "number" or "integer" => "Integer", "decimal" or "currency" => "Decimal", "boolean" or "checkbox" => "Boolean", "date" => "Date", "datetime" => "DateTime", "email" => "Email", "url" => "Url", _ => "Text" };
                        fields.Add(new DraftField(fieldKey, label, type));
                    }
                if (!KeyPattern.IsMatch(key) || fields.Count is < 1 or > 100 || string.IsNullOrWhiteSpace(name) || name.Length > 120)
                    throw new ArgumentException($"Data model '{name}' needs a valid key and at least one field.");
                if (fields.Any(field => !KeyPattern.IsMatch(field.Key) || string.IsNullOrWhiteSpace(field.Label) || field.Label.Length > 120)
                    || fields.Select(field => field.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count() != fields.Count)
                    throw new ArgumentException($"Data model '{name}' has an invalid field key.");
                models.Add(new DraftModel(key, name, fields));
            }
        }
        if (models.Count == 0) models.Add(new DraftModel("records", "Records", [new DraftField("name", "Name", "Text")]));
        if (models.Count > 20 || models.Select(model => model.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count() != models.Count)
            throw new ArgumentException("A template can contain up to 20 uniquely keyed data models.");
        return models;
    }

    private static string? GetString(JsonElement element, string property) => element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    private static bool GetBoolean(JsonElement element, string property, bool fallback) => element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False ? value.GetBoolean() : fallback;
    private static int GetInteger(JsonElement element, string property, int fallback) => element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out var value) && value.TryGetInt32(out var result) ? result : fallback;

    private static Dictionary<string, string> ReadFigmaStyle(JsonElement element)
    {
        var result = new Dictionary<string, string>();
        if (!element.TryGetProperty("figmaStyle", out var style) || style.ValueKind != JsonValueKind.Object) return result;
        AddColor("color", "color");
        AddColor("backgroundColor", "background-color");
        AddPixel("fontSize", "font-size", 7, 96);
        AddPixel("borderRadius", "border-radius", 0, 256);
        if (style.TryGetProperty("fontWeight", out var weight) && weight.TryGetInt32(out var weightValue) && weightValue is >= 100 and <= 900)
            result["font-weight"] = weightValue.ToString(CultureInfo.InvariantCulture);
        return result;

        void AddColor(string source, string target)
        {
            var color = GetString(style, source);
            if (color is not null && Regex.IsMatch(color, "^#[0-9a-fA-F]{6}$")) result[target] = color;
        }
        void AddPixel(string source, string target, double min, double max)
        {
            if (style.TryGetProperty(source, out var value) && value.TryGetDouble(out var number) && number >= min && number <= max)
                result[target] = number.ToString("0.##", CultureInfo.InvariantCulture) + "px";
        }
    }

    private static void AddFigmaPosition(Dictionary<string, string> styling, JsonElement figmaStyle, double frameWidth, double frameHeight)
    {
        var x = Math.Clamp(GetNumber(figmaStyle, "x"), 0, frameWidth);
        var y = Math.Clamp(GetNumber(figmaStyle, "y"), 0, frameHeight);
        var width = Math.Clamp(GetNumber(figmaStyle, "width"), 0, frameWidth - x);
        var height = Math.Clamp(GetNumber(figmaStyle, "height"), 0, frameHeight - y);
        if (width <= 0 || height <= 0) return;

        styling["position"] = "absolute";
        styling["left"] = Percent(x, frameWidth);
        styling["top"] = Percent(y, frameHeight);
        styling["width"] = Percent(width, frameWidth);
        styling["height"] = Percent(height, frameHeight);
    }

    private static string Percent(double value, double total) =>
        (value / total * 100).ToString("0.####", CultureInfo.InvariantCulture) + "%";

    private static void AddFigmaFontScaling(Dictionary<string, string> styling, JsonElement figmaStyle, double frameWidth)
    {
        var fontSize = GetNumber(figmaStyle, "fontSize");
        if (fontSize is >= 7 and <= 96 && frameWidth > 0)
            styling["font-size"] = (fontSize / frameWidth * 100).ToString("0.####", CultureInfo.InvariantCulture) + "cqw";
    }

    private static JsonElement GetProperty(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value) ? value : default;

    private static double GetNumber(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value)
        && value.TryGetDouble(out var number) && double.IsFinite(number) ? Math.Clamp(number, 0, 10000) : 0;

    private sealed record DraftPage(string Key, string Title, string Description, string Type, List<JsonElement> Elements);
    private sealed record DraftModel(string Key, string Name, List<DraftField> Fields);
    private sealed record DraftField(string Key, string Label, string Type);
}
