namespace SorteFederation.Logger.Options;

public class SendMailOptions
{
    public bool Enabled { get; set; }
    public string EmailFrom { get; set; }
    public string EmailTo { get; set; }
    public string MailServer { get; set; }
    public bool EnableSsl { get; set; }
    public int Port { get; set; }
    public string Subject { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
