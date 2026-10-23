using System.Net.Mail;
using System.Net;

namespace ANTSoftFederation.IdentityServer.Services.Email;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    { 
        var smtpClient = new SmtpClient(_configuration["EmailSettings:Host"])
        {
            Port = int.Parse(_configuration["EmailSettings:Port"]),
            Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["EmailSettings:From"]),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }
}


