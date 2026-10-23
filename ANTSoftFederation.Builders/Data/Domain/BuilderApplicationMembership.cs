namespace SorteFederation.Builders.Data.Domain;

/// <summary>Application-scoped access and role for a tenant-wide Builder identity.</summary>
public sealed class BuilderApplicationMembership : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public Guid UserId { get; set; }
    public required string Role { get; set; }
    public bool IsActive { get; set; } = true;
    public BuilderApplicationUser User { get; set; } = null!;
}
