namespace SorteFederation.DesignerStudio.Data.Entities;

public sealed class TemplatePackageRelease
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TemplatePackageId { get; set; }
    public TemplatePackage TemplatePackage { get; set; } = null!;
    public string Version { get; set; } = string.Empty;
    public TemplateReleaseStatus Status { get; set; } = TemplateReleaseStatus.Draft;
    public string ManifestJson { get; set; } = "{}";
    public string PackagePath { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? ChangeNotes { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? ReviewedByUserId { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}

public enum TemplateReleaseStatus
{
    Draft,
    Submitted,
    Published,
    Rejected,
    Withdrawn
}
