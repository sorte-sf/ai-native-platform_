
using SorteFederation.Shared.Models;
using MailKit.Security;
using MimeKit;  

using ContentType = MimeKit.ContentType;

namespace SorteFederation.Builders.Services;

public interface IMailKitService
{
    Task<bool> SendAsync(MailData mailData, CancellationToken ct = default);
    Task<bool> SendWithAttachedFileAsync(MailDataWithAttachments mailData, CancellationToken ct = default);
    Task<bool> SendWithAttachmentsAsync(MailData mailData, Multipart multipart, CancellationToken ct = default);
}


public sealed class MailKitService : IMailKitService
{
    private readonly ILogger<MailKitService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MailKitService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MailKitService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<bool> SendAsync(MailData mailData, CancellationToken ct = default)
    {
        try
        {
            var settings = await LoadSettingsAsync();

            if (!ValidateSettings(settings))
                return false;

            var mail = new MimeMessage();
            AddMailAddresses(mail, mailData, settings);

            var bodyBuilder = new BodyBuilder { HtmlBody = mailData.Body };
            mail.Subject = mailData.Subject;
            mail.Body = bodyBuilder.ToMessageBody();

            return await SendMailAsync(mail, settings, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending email to {To}.", mailData.To);
            return false;
        }
    }

    public async Task<bool> SendWithAttachedFileAsync(MailDataWithAttachments mailData, CancellationToken ct = default)
    {
        try
        {
            var settings = await LoadSettingsAsync();

            if (!ValidateSettings(settings))
                return false;

            var mail = new MimeMessage();
            AddMailWithAttachmentAddresses(mail, mailData, settings);

            var bodyBuilder = new BodyBuilder { HtmlBody = mailData.Body };

            if (mailData.Attachments != null)
            {
                foreach (var attachment in mailData.Attachments.Where(a => a.Length > 0))
                {
                    using var memoryStream = new MemoryStream();
                    await attachment.CopyToAsync(memoryStream, ct);
                    bodyBuilder.Attachments.Add(
                        attachment.FileName,
                        memoryStream.ToArray(),
                        ContentType.Parse(attachment.ContentType));
                }
            }

            mail.Subject = mailData.Subject;
            mail.Body = bodyBuilder.ToMessageBody();

            return await SendMailAsync(mail, settings, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending email with attachments to {To}.", mailData.To);
            return false;
        }
    }

    public async Task<bool> SendWithAttachmentsAsync(MailData mailData, Multipart multipart, CancellationToken ct = default)
    {
        try
        {
            var settings = await LoadSettingsAsync();

            if (!ValidateSettings(settings))
                return false;

            var email = new MimeMessage
            {
                Subject = mailData.Subject,
                Body = multipart
            };

            email.From.Add(new MailboxAddress(settings.DisplayName, settings.From));
            email.To.Add(MailboxAddress.Parse(mailData.To));

            return await SendMailAsync(email, settings, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending email with multipart attachments to {To}.", mailData.To);
            return false;
        }
    }
     
    private async Task<EmailSettings> LoadSettingsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var loadEmailSettings = scope.ServiceProvider.GetRequiredService<LoadEmailSettings>();
        return await loadEmailSettings.EmailSettingAsync();
    }

    private bool ValidateSettings(EmailSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            _logger.LogError("SMTP Host is not configured.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.UserName) || string.IsNullOrWhiteSpace(settings.Password))
        {
            _logger.LogError("SMTP credentials are not configured.");
            return false;
        }

        return true;
    }

    private SecureSocketOptions ResolveSecureOption(EmailSettings settings)
    {
        if (settings.EnableSsl == true)
            return SecureSocketOptions.SslOnConnect;

        if (settings.UseStartTls == true)
            return SecureSocketOptions.StartTls;

        return SecureSocketOptions.Auto;
    }

    private async Task<bool> SendMailAsync(MimeMessage mail, EmailSettings settings, CancellationToken ct)
    {
        var secureOption = ResolveSecureOption(settings);

        using var smtp = new MailKit.Net.Smtp.SmtpClient();

        smtp.Connected += (_, _) =>
            _logger.LogInformation("Connected to SMTP server {Host}:{Port}.", settings.Host, settings.Port);

        smtp.Authenticated += (_, _) =>
            _logger.LogInformation("Authenticated to SMTP server {Host}:{Port}.", settings.Host, settings.Port);

        smtp.MessageSent += (_, e) =>
            _logger.LogInformation("Mail successfully sent. Response: {Response}", e.Response);

        smtp.Disconnected += (_, _) =>
            _logger.LogInformation("Disconnected from SMTP server {Host}:{Port}.", settings.Host, settings.Port);

        try
        {
            await smtp.ConnectAsync(settings.Host, settings.Port ?? 587, secureOption, ct);

            if (!smtp.IsConnected)
            {
                _logger.LogError("Failed to connect to SMTP server {Host}:{Port}.", settings.Host, settings.Port);
                return false;
            }

            await smtp.AuthenticateAsync(settings.UserName, settings.Password, ct);

            if (!smtp.IsAuthenticated)
            {
                _logger.LogError("Failed to authenticate with SMTP server {Host}:{Port}.", settings.Host, settings.Port);
                return false;
            }

            await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: _ => TimeSpan.FromSeconds(2),
                    onRetry: (ex, ts, attempt, _) =>
                        _logger.LogWarning(
                            "Retry {Attempt}/3 sending email due to: {Message}. Waiting {Seconds}s.",
                            attempt, ex.Message, ts.TotalSeconds))
                .ExecuteAsync(() => smtp.SendAsync(mail, ct));

            await smtp.DisconnectAsync(true, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via {Host}:{Port}.", settings.Host, settings.Port);
            return false;
        }
    }

    private static void AddMailAddresses(MimeMessage mail, MailData mailData, EmailSettings settings)
    {
        var fromAddress = mailData.From ?? settings.From;
        var displayName = mailData.DisplayName ?? settings.DisplayName;

        mail.From.Add(new MailboxAddress(displayName, fromAddress));
        mail.Sender = new MailboxAddress(displayName, fromAddress);

        if (!string.IsNullOrWhiteSpace(mailData.To))
            mail.To.Add(MailboxAddress.Parse(mailData.To));

        if (!string.IsNullOrWhiteSpace(mailData.ReplyTo))
            mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));
    }

    private static void AddMailWithAttachmentAddresses(MimeMessage mail, MailDataWithAttachments mailData, EmailSettings settings)
    {
        var fromAddress = mailData.From ?? settings.From;
        var displayName = mailData.DisplayName ?? settings.DisplayName;

        mail.From.Add(new MailboxAddress(displayName, fromAddress));
        mail.Sender = new MailboxAddress(displayName, fromAddress);

        if (!string.IsNullOrWhiteSpace(mailData.To))
            mail.To.Add(MailboxAddress.Parse(mailData.To));

        if (!string.IsNullOrWhiteSpace(mailData.ReplyTo))
            mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));

        AddRecipients(mailData.Bcc, mail.Bcc);
        AddRecipients(mailData.Cc, mail.Cc);
    }

    private static void AddRecipients(List<string>? recipients, InternetAddressList addressList)
    {
        if (recipients is null) return;

        foreach (var email in recipients.Where(e => !string.IsNullOrWhiteSpace(e)))
        {
            addressList.Add(MailboxAddress.Parse(email.Trim()));
        }
    }


}
 
