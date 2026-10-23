namespace ANTSoftFederation.IdentityServer.Services.Custom;

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
            "sqlserver" or "mssql" => DatabaseType.SqlServer,
            _ => DatabaseType.PostgreSQL
        };
    }

    public string GetConnectionString()
    {
        var dbType = GetDatabaseType();
        return dbType switch
        {
            DatabaseType.PostgreSQL => _configuration.GetConnectionString("PostgreSQL")!,
            DatabaseType.MySQL => _configuration.GetConnectionString("MySQL")!,
            DatabaseType.SqlServer => _configuration.GetConnectionString("SqlServer")!,
            _ => _configuration.GetConnectionString("SqlServer")!
        };
    }

    public void ConfigureDatabase(DbContextOptionsBuilder options)
    {
        var dbType = GetDatabaseType();
        var connectionString = GetConnectionString();

        switch (dbType)
        {
            case DatabaseType.PostgreSQL:
                options.UseNpgsql(connectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "public"));
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                _logger.LogInformation("Configured PostgreSQL database");
                break;

            //case DatabaseType.MySQL:
            //    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
            //        o => o.MigrationsHistoryTable("__EFMigrationsHistory"));
            //    _logger.LogInformation("Configured MySQL database");
            //    break;

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
