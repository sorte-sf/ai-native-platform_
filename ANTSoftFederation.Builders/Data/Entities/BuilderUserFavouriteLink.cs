using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Data.Entities;

/// <summary>A published-app user's saved, permission-revalidated generated view.</summary>
public sealed class BuilderUserFavouriteLink : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public required string SpecificationJson { get; set; }
    public required string PublishedVersion { get; set; }
}
