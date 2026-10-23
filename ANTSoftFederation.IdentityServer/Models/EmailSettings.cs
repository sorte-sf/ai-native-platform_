namespace ANTSoftFederation.IdentityServer.Models;

public class EmailSettings
{
    public string? DisplayName { get; set; }

    public string? From { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? Host { get; set; }

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public bool UseStartTls { get; set; } = false;
}
