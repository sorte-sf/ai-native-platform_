namespace SorteFederation.Builders.Data.Domain;

/// <summary>Controls access of a tenant's Builder identity to the application design workspace.</summary>
public sealed class BuilderWorkspaceAccess : BuilderEntity
{
    public Guid UserId { get; set; }
    public required string Role { get; set; }
    public bool IsActive { get; set; } = true;
    public int SecurityVersion { get; set; } = 1;
    public BuilderApplicationUser User { get; set; } = null!;
}
