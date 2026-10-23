using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Contracts;

public sealed record TemplateManifest(int Version, IReadOnlyList<TemplateObject> Objects,
    IReadOnlyList<TemplateScreen> Screens, IReadOnlyList<TemplateWorkflow>? Workflows = null,
    IReadOnlyList<TemplateNavigationItem>? Navigation = null,
    TemplateApplicationShell? Shell = null, TemplateDesignTheme? Theme = null,
    IReadOnlyList<TemplateRelationship>? Relationships = null);
public sealed record TemplateObject(string Key, string Name, IReadOnlyList<TemplateField> Fields,
    IReadOnlyList<TemplateForm> Forms);
public sealed record TemplateField(string Key, string Label, BuilderFieldType Type,
    bool Required = false, bool Searchable = false, string? ConfigurationJson = null);
public sealed record TemplateForm(string Key, string Name, string FormConfigJson);
public sealed record TemplateScreen(string Key, string Title, string ObjectKey,
    string HtmlContent, string? FormKey = null, string? Icon = null, string? DefinitionJson = null);
public sealed record TemplateWorkflow(string Key, string Name, string DefinitionJson);
public sealed record TemplateNavigationItem(string Key, string Label, string? Icon, string? ScreenKey,
    IReadOnlyList<TemplateNavigationItem>? Children = null);
public sealed record TemplateRelationship(string Key, string Name, string SourceObjectKey, string TargetObjectKey, string Cardinality);
public sealed record TemplateApplicationShell(string? Brand, string? LogoUrl, string? ProfileName, string? ProfileRole,
    string? Language, bool? ShowSearch, bool? ShowMegaMenu, bool? ShowNotifications, bool? ShowSettings,
    bool? ShowThemeToggle, string? ThemeMode, bool? SidebarCollapsed, string? SidebarPosition, int? SidebarWidth,
    string? HeaderPosition, int? HeaderHeight, string? SidebarBackground, string? SidebarTextColor,
    string? HeaderBackground, string? HeaderTextColor);
public sealed record TemplateDesignTheme(string? AccentColor, string? PageBackground, string? TextColor,
    string? FontFamily, double? FontSize);
