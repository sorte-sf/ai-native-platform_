using System.ComponentModel.DataAnnotations;

namespace SorteFederation.Models;

public sealed class PortalTenant(string name, string databaseName, string connectionString, DatabaseType databaseType = DatabaseType.PostgreSQL)
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    [MaxLength(80)] public string Code { get; set; } = string.Empty;
    [MaxLength(180)] public string Name { get; set; } = name;
    [MaxLength(128)] public string DatabaseName { get; set; } = databaseName;

    public DatabaseType DatabaseType { get; set; } = databaseType;

    [MaxLength(2000)] public string ConnectionString { get; set; } = connectionString;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    [MaxLength(20)] public string ThemeKey { get; set; } = "default";
    [MaxLength(10)] public string ThemeMode { get; set; } = "light";
}
