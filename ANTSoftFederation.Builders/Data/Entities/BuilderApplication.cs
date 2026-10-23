
namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderApplication : BuilderEntity
{
    public string VersionNo { get; set; } = "0.0.1";
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public BuilderApplicationType ApplicationType { get; set; } = BuilderApplicationType.Web;
    public Guid? TemplateId { get; set; }
    public int? TemplateVersion { get; set; }
    public Guid? TemplateReleaseId { get; set; }
    public string? AppliedTemplateVersion { get; set; }
    public string? ThemeJson { get; set; }
    public BuilderTemplate? Template { get; set; }
    public DefinitionStatus Status { get; set; } = DefinitionStatus.Draft;
    public string PublishedVersion { get; set; } = "0.0.1";
    public string[]? Slugs { get; set; } = [];

    public Guid? CompanyId { get; set; }
    public Tenant Company { get; set; }

    public ICollection<BuilderObject> Objects { get; set; } = [];
    public ICollection<BuilderApplicationVersion> Versions { get; set; } = [];
    public ICollection<BuilderApplicationMenuItem> MenuItems { get; set; } = [];
    public ICollection<BuilderApplicationScreen> Screens { get; set; } = [];
    public ICollection<BuilderApplicationModule> Modules { get; set; } = [];
}
