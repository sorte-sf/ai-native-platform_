 
namespace ANTSoftFederation.IdentityServer.Services.Email;

public interface IMailKitService
{
    Task<bool> SendAsync(MailData mailData, CancellationToken ct);

    Task<bool> SendWithAttachmentsAsync(MailDataWithAttachments mailData, CancellationToken ct);
}

public class MailKitService : IMailKitService
{
    private readonly EmailSettings _settings;
    private readonly IConfiguration _config;

    public MailKitService(IConfiguration config)
    {
        _config = config;
        var section = _config.GetSection("EmailSettings");
        _settings = section.Get<EmailSettings>();
    }

    public async Task<bool> SendWithAttachmentsAsync(MailDataWithAttachments mailData, CancellationToken ct = default)
    {
        try
        {
            var mail = new MimeMessage();

            #region Sender / Receiver

            mail.From.Add(new MailboxAddress(_settings.DisplayName, mailData.From ?? _settings.From));
            mail.Sender = new MailboxAddress(mailData.DisplayName ?? _settings.DisplayName, mailData.From ?? _settings.From);

            //foreach (string mailAddress in mailData.To)
            //mail.To.Add(MailboxAddress.Parse(mailAddress));

            mail.To.Add(MailboxAddress.Parse(mailData.To));

            if (!string.IsNullOrEmpty(mailData.ReplyTo))
                mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));

            if (mailData.Bcc != null)
            {
                foreach (string mailAddress in mailData.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
                    mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
            }

            if (mailData.Cc != null)
            {
                foreach (string mailAddress in mailData.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
                    mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
            }
            #endregion

            #region Content

            var body = new BodyBuilder();
            mail.Subject = mailData.Subject;

            /* (mailData.Attachments != null)
            {
                byte[] attachmentFileByteArray;
                foreach (IFormFile attachment in mailData.Attachments)
                {
                    if (attachment.Length > 0)
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            attachment.CopyTo(memoryStream);
                            attachmentFileByteArray = memoryStream.ToArray();
                        }
                        body.Attachments.Add(attachment.FileName, attachmentFileByteArray, ContentType.Parse(attachment.ContentType));
                    }
                }
            }*/

            body.HtmlBody = mailData.Body;
            mail.Body = body.ToMessageBody();

            #endregion

            #region Send Mail

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            if (_settings.EnableSsl)
            {
                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect, ct);
            }
            else if (_settings.UseStartTls)
            {
                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
            }

            await smtp.AuthenticateAsync(_settings.UserName, _settings.Password, ct);

            // await smtp.SendAsync(mail, ct);
            // await smtp.DisconnectAsync(true, ct);

            await Policy
                .Handle<Exception>()
                .Or<CommandException>()
                .WaitAndRetry(5, r => TimeSpan.FromSeconds(2), (ex, ts) =>
                {
                    Console.WriteLine("Error sending mail. Retrying in 2 sec.");
                })
                .Execute(() => smtp.SendAsync(mail, ct))
                .ContinueWith(_ =>
                {
                    Console.WriteLine("Notification mail sent to {Recipient}.", mailData.To);
                });

            await smtp.DisconnectAsync(true, ct);
            return true;

            #endregion

        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> SendAsync(MailData mailData, CancellationToken ct = default)
    {
        try
        {
            var mail = new MimeMessage();

            mail.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
            mail.Sender = new MailboxAddress(mailData.DisplayName, _settings.From);

            mail.To.Add(MailboxAddress.Parse(mailData.To));


            //if (!string.IsNullOrEmpty(mailData.ReplyTo))
            //    mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));

            var body = new BodyBuilder();
            mail.Subject = mailData.Subject;
            body.HtmlBody = mailData.Body;
            mail.Body = body.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            if (_settings.EnableSsl)
            {
                await smtp.ConnectAsync(_settings.Host, _settings.Port);

                //await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect, ct);
            }
            else if (_settings.UseStartTls)
            {
                await smtp.ConnectAsync(_settings.Host, _settings.Port);
                //await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
            }

            await smtp.AuthenticateAsync(_settings.UserName, _settings.Password, ct);

            var res = await smtp.SendAsync(mail, ct);

            await smtp.DisconnectAsync(true, ct);

            return res == "OK" ? true : false;

        }
        catch (Exception ex)
        {
            return false;
        }
    }

}
