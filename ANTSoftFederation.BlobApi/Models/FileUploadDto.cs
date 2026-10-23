namespace SorteFederation.BlobApi.Models;

public class FileUploadDto
{
    public string RelationId { get; set; }
    public string FilePath{ get; set; }
    public IFormFile Files { get; set; }
}
