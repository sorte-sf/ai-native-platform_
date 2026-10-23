
namespace SorteFederation.BlobApi.DataDbContext.Entities.Base;

public abstract class BaseEntity
{ 
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
}