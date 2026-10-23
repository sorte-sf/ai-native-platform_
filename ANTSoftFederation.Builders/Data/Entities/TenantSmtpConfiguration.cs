using SorteFederation.EncryptEFCoreColumn;

namespace SorteFederation.Builders.Data.Domain;

public class TenantSmtpConfiguration : BuilderEntity
{
    [EncryptColumn]
    public string Host { get; set; } = string.Empty;
    public int? Port { get; set; } = 587;
    [EncryptColumn]
    public string? UserName { get; set; }
    [EncryptColumn]
    public string From { get; set; } = string.Empty;
    [EncryptColumn]
    public string? DisplayName { get; set; }
    [EncryptColumn]
    public string? Password { get; set; }
    public bool? EnableSsl { get; set; } = false;
    public bool? UseStartTls { get; set; } = true;
    public Tenant Tenant { get; set; } = null!;
}
