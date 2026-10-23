namespace SorteFederation.Shared.TemplateCatalog;

/// <summary>Notification that an immutable Designer Studio template release is available.</summary>
public sealed record TemplateReleasePublishedEvent(
    Guid TemplateId,
    string TemplateKey,
    string TemplateName,
    string Category,
    string? Description,
    Guid ReleaseId,
    string Version,
    DateTimeOffset PublishedAt,
    string ManifestJson);
