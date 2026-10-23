
namespace SorteFederation.ANTApplications.Services.Custom;

public class DatabaseService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public DatabaseType GetDatabaseType()
    {
        var dbType = _configuration["DbType"];
        return dbType?.ToLower() switch
        {
            "postgres" or "postgresql" => DatabaseType.PostgreSQL,
            "mysql" => DatabaseType.MySQL,
            "sqlite" => DatabaseType.Sqlite,
            "sqlserver" or "mssql" => DatabaseType.SqlServer,
            _ => DatabaseType.SqlServer
        };
    }

    public string GetConnectionString()
    {
        var dbType = GetDatabaseType();
        return dbType switch
        {
            DatabaseType.PostgreSQL => GetConnectionString("Antsoft-Masterdb", "PostgreSQL"),
            DatabaseType.MySQL => _configuration.GetConnectionString("MySQL")!,
            DatabaseType.Sqlite => _configuration.GetConnectionString("Sqlite")!,
            DatabaseType.SqlServer => _configuration.GetConnectionString("SqlServer")!,
            _ => _configuration.GetConnectionString("PostgreSQL")!
        };
    }

    private string GetConnectionString(string aspireName, string fallbackName)
    {
        return _configuration.GetConnectionString(aspireName)
            ?? _configuration.GetConnectionString("SortePortalDb2026")
            ?? _configuration.GetConnectionString("AntMasterDatabase")
            ?? _configuration.GetConnectionString(fallbackName)
            ?? throw new InvalidOperationException(
                $"Missing PostgreSQL connection string. Expected '{aspireName}' (Aspire) or '{fallbackName}'.");
    }

    public void ConfigureDatabase(DbContextOptionsBuilder options)
    {
        var dbType = GetDatabaseType();
        var connectionString = GetConnectionString();

        switch (dbType)
        {
            case DatabaseType.PostgreSQL:
                options.UseNpgsql(connectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "public"));
                _logger.LogInformation("Configured PostgreSQL database");
                break;

            /*case DatabaseType.MySQL:
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    o => o.MigrationsHistoryTable("__EFMigrationsHistory"));
                _logger.LogInformation("Configured MySQL database");
                break;*/

            case DatabaseType.Sqlite:
                options.UseSqlite(connectionString);
                _logger.LogInformation("Configured SqLite database");
                break;

            case DatabaseType.SqlServer:
                options.UseSqlServer(connectionString);
                _logger.LogInformation("Configured SQL Server database");
                break;

            default:
                options.UseNpgsql(connectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "public"));
                _logger.LogInformation("Configured PostgreSQL database");
                break;
        }
    }
}

 
