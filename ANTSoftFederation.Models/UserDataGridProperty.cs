namespace SorteFederation.Models;

public sealed class UserDataGridProperty(Guid userId, string module, string groupedFieldsJson)
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; } = userId;
    public User User { get; set; } = null!;
    [MaxLength(120)] public string Module { get; set; } = module;
    public string GroupedFieldsJson { get; set; } = groupedFieldsJson;
    public string HiddenColumnsJson { get; set; } = "[]";
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
