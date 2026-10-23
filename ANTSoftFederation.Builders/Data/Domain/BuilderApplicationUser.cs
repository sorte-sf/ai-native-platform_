namespace SorteFederation.Builders.Data.Domain;

/// <summary>A tenant-wide Builder identity. Application access is granted separately by membership.</summary>
public sealed class BuilderApplicationUser : BuilderEntity
{
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public int SecurityVersion { get; set; } = 1;
    public DateTimeOffset? DisabledAt { get; set; }
    public ICollection<BuilderApplicationMembership> Memberships { get; set; } = [];
}
