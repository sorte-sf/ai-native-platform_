namespace SorteFederation.ANTApplications.DTOs.Base;

public class BaseListDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}