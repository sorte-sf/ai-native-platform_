namespace SorteFederation.Builders.Data.Domain;

/// <summary>An immutable, published snapshot of a reusable builder template.</summary>
public sealed class BuilderTemplateRelease
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TemplateId { get; set; }
    public BuilderTemplate? Template { get; set; }
    public required string ReleaseVersion { get; set; }
    public int ManifestVersion { get; set; }
    public string? ManifestJson { get; set; }
    public string? ThemeJson { get; set; }
    public required string HtmlContent { get; set; }
    public required string ScssContent { get; set; }
    public string? Description { get; set; }
    public string? PreviewImageUrl { get; set; }
    public string? BaseTemplate { get; set; }
    public DateTimeOffset ReleasedAt { get; set; } = DateTimeOffset.UtcNow;
}
