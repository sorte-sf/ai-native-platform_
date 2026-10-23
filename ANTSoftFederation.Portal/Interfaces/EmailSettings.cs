namespace SorteFederation.Portal.Interfaces;

public class EmailSettings
{
    public string DisplayName { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public bool UseStartTls { get; set; } = false;
}