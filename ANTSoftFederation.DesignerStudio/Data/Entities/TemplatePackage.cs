namespace SorteFederation.DesignerStudio.Data.Entities;

public sealed class TemplatePackage
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DraftDefinitionJson { get; set; } = "{\"formatVersion\":1,\"page\":{\"title\":\"\",\"description\":\"\"},\"elements\":[]}";
    public string OwnerUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<TemplatePackageRelease> Releases { get; set; } = new List<TemplatePackageRelease>();
}
