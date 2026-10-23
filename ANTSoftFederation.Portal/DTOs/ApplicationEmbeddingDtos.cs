
namespace SorteFederation.ANTApplications.DTOs;

public class CreateApplicationEmbeddingDto : BaseCreateDto
{
    public required float[] AppVector { get; set; }
    public required Guid ApplicationDocId { get; set; }
}

public class UpdateApplicationEmbeddingDto : BaseUpdateDto
{
    public required float[] AppVector { get; set; }
    public required Guid ApplicationDocId { get; set; }
}

public class ApplicationEmbeddingDto : BaseDto
{
    public required float[] AppVector { get; set; }
    public required Guid ApplicationDocId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ApplicationEmbeddingListDto : BaseListDto
{
    public required Guid ApplicationDocId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}