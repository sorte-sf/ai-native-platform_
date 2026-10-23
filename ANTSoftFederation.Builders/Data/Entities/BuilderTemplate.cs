namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderTemplate : BuilderEntity
{
    public string? Key { get; set; }
    public string Category { get; set; } = "General";
    public string BlueprintVersion { get; set; } = "1.0.0";
    public string? PreviewImageUrl { get; set; }
    public int ManifestVersion { get; set; } = 1;
    public string? ManifestJson { get; set; }
    public bool IsBuiltIn { get; set; }
    public string? BaseTemplate { get; set; }
    public string? ThemeJson { get; set; }
    public ICollection<BuilderTemplateRelease> Releases { get; set; } = [];
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string HtmlContent { get; set; }
    public required string ScssContent { get; set; }
    public ICollection<BuilderApplication> Applications { get; set; } = [];
}
