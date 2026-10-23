
namespace SorteFederation.ANTApplications.Services.Custom;

public interface IDbInitializerService
{
    void Initialize(); 
    void SeedApplications(); 
}

public class DbInitializerService : IDbInitializerService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ISecurityService _securityService;
    public readonly ILogger<DbInitializerService> _logger;

    public DbInitializerService(
           IServiceScopeFactory scopeFactory,
           ILogger<DbInitializerService> logger,
            ISecurityService securityService
        )
    {
        _scopeFactory = scopeFactory;
        _securityService = securityService;
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            _logger.LogInformation("Ensure User Management Database");

            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ANTSoftPortalDbContext>();

            if (context.Database.CanConnect())
            {
                var pending = context.Database.GetPendingMigrations();
                if (!pending.Any())
                {
                    _logger.LogInformation("No pending migrations found");
                    return;
                }
            }

            Policy
                .Handle<Exception>()
                .WaitAndRetry(5, r => TimeSpan.FromSeconds(5), (ex, ts) =>
                {
                    _logger.LogError(ex, "Error connecting to DB. Retrying in 5 sec.");
                })
                .Execute(() => context.Database.Migrate());

            _logger.LogInformation("User Management Database available");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException.Message);
            throw new InvalidOperationException(ex.InnerException.Message.ToString());
        }
    }
     
    public void SeedApplications()
    {
        try
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ANTSoftPortalDbContext>();

            if (!context.ProjectApplications.Any())
            {
                var appData = new List<ProjectApplication>
                    {
                       new ProjectApplication {
                           Id = Guid.CreateVersion7(),
                           Name = "ANTSoft Portal Management",
                           AppKey = $"api_{Guid.Parse("9111365E-0B80-430D-9B85-6A6716886604").ToString("N")}",
                           DatabaseType = DatabaseType.PostgreSQL,
                           DefaultTheme = "deafult",

                           RecordState = RecordState.Active,
                           CreatedBy = "system",
                           CreatedAt = DateTime.UtcNow
                       },
                        
                };

                context.ProjectApplications.AddRange(appData);
                context.SaveChanges();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException.Message);
            throw new InvalidOperationException(ex.InnerException.Message.ToString());
        }
    }
     
}
