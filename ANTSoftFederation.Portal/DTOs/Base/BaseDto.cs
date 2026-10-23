namespace SorteFederation.ANTApplications.DTOs.Base;

public class BaseDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}