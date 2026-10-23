using SorteFederation.Shared.Models;

namespace SorteFederation.Builders.Services;

public class LoadEmailSettings
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LoadEmailSettings(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<EmailSettings> EmailSettingAsync()
    {
        EmailSettings emailSettings = new();
        try
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<BuilderDbContext>();

            emailSettings = await context.TenantSmtpConfigurations 
                                         .Select(k => new EmailSettings
                                         {
                                             Host = k.Host,
                                             Port = k.Port,
                                             From = k.From,
                                             DisplayName = k.DisplayName,
                                             UserName = k.UserName,
                                             Password = k.Password,
                                             EnableSsl = k.EnableSsl ,
                                             UseStartTls = k.UseStartTls
                                         })
                                        .SingleOrDefaultAsync();
            return emailSettings;

        }
        catch (Exception ex)
        {
            return emailSettings;
        }

    }
}
