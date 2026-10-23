using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters; 
using System.Net;
using SorteFederation.Logger.Options;
using Serilog.Sinks.MSSqlServer;

namespace SorteFederation.Logger;

public static class Extensions
{
    private const string LoggerSectionName = "logger";
    private const string AppSectionName = "app";
    internal static LoggingLevelSwitch LoggingLevelSwitch = new();

    public static IHostBuilder UseANTLogging(this IHostBuilder hostBuilder,
        Action<HostBuilderContext, LoggerConfiguration> configure = null, string loggerSectionName = LoggerSectionName,
        string appSectionName = AppSectionName)
        => hostBuilder
            .ConfigureServices(services => services.AddSingleton<ILoggingService, LoggingService>())
            .UseSerilog((context, loggerConfiguration) =>
            {
                if (string.IsNullOrWhiteSpace(loggerSectionName))
                {
                    loggerSectionName = LoggerSectionName;
                }

                if (string.IsNullOrWhiteSpace(appSectionName))
                {
                    appSectionName = AppSectionName;
                }

                var loggerOptions = context.Configuration.GetSection(loggerSectionName).Get<LoggerOptions>();
                var appOptions = context.Configuration.GetSection(appSectionName).Get<AppOptions>();

                // var loggerOptions = context.Configuration.Bind<LoggerOptions>(loggerSectionName);
                // var appOptions = context.Configuration.BindOptions<AppOptions>(appSectionName);

                MapOptions(loggerOptions, appOptions, loggerConfiguration, context.HostingEnvironment.EnvironmentName);
                configure?.Invoke(context, loggerConfiguration);
            });


    public static IEndpointConventionBuilder MapLogLevelHandler(this IEndpointRouteBuilder builder, string endpointRoute = "~/logging/level")
        => builder.MapPost(endpointRoute, LevelSwitch);

    private static void MapOptions(LoggerOptions loggerOptions, AppOptions appOptions,
        LoggerConfiguration loggerConfiguration, string environmentName)
    {
        LoggingLevelSwitch.MinimumLevel = GetLogEventLevel(loggerOptions.Level);

        loggerConfiguration.Enrich.FromLogContext()
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .Enrich.WithProperty("Environment", environmentName)
            //.Enrich.WithProperty("Application", appOptions.Service)
            //.Enrich.WithProperty("Instance", appOptions.Instance)
            .Enrich.WithProperty("Version", appOptions.Version);

        foreach (var (key, value) in loggerOptions.Tags ?? new Dictionary<string, object>())
        {
            loggerConfiguration.Enrich.WithProperty(key, value);
        }

        foreach (var (key, value) in loggerOptions.MinimumLevelOverrides ?? new Dictionary<string, string>())
        {
            var logLevel = GetLogEventLevel(value);
            loggerConfiguration.MinimumLevel.Override(key, logLevel);
        }

        loggerOptions.ExcludePaths?.ToList().ForEach(p => loggerConfiguration.Filter
            .ByExcluding(Matching.WithProperty<string>("RequestPath", n => n.EndsWith(p))));

        loggerOptions.ExcludeProperties?.ToList().ForEach(p => loggerConfiguration.Filter
            .ByExcluding(Matching.WithProperty(p)));

        Configure(loggerConfiguration, loggerOptions);
    }

    private static void Configure(LoggerConfiguration loggerConfiguration, LoggerOptions options)
    {
        var consoleOptions = options.Console ?? new ConsoleOptions();

        var fileOptions = options.File ?? new Options.FileOptions();

        var elkOptions = options.Elk ?? new ElkOptions();

        var seqOptions = options.Seq ?? new SeqOptions();

        var lokiOptions = options.Loki ?? new LokiOptions();

        var elkLogstashOptions = options.ElkLog ?? new ELKLogstashOptions();

        var msSqlDBOptions = options.Mssqldb ?? new MsSqDbOptions();

        var sendMailOptions = options.SendMail ?? new SendMailOptions();

        var mongoDBOptions = options.Mongodb ?? new MongoDbOptions();

        //var kafkaOptions = options.KafkaOptions ?? new KafkaOptions();

        if (consoleOptions.Enabled)
        {
            // loggerConfiguration.WriteTo.Console(theme: AnsiConsoleTheme.Code);
            loggerConfiguration.WriteTo.Console();
        }

        if (fileOptions.Enabled)
        {
            var path = string.IsNullOrWhiteSpace(fileOptions.Path) ? "logs/logs.txt" : fileOptions.Path;

            if (!Enum.TryParse<RollingInterval>(fileOptions.Interval, true, out var interval))
            {
                interval = RollingInterval.Day;
            }

            loggerConfiguration.WriteTo.File(path, rollingInterval: interval);
        }

        //if (kafkaOptions.Enabled)
        //{
        //    loggerConfiguration.WriteTo.Kafka(
        //        batchSizeLimit: kafkaOptions.BatchSizeLimit,
        //        period: kafkaOptions.Period,
        //        bootstrapServers: kafkaOptions.BootstrapServers,
        //        saslUsername: kafkaOptions.SaslUsername,
        //        saslPassword: kafkaOptions.SaslPassword,
        //        topic: kafkaOptions.Topic,
        //        sslCaLocation: kafkaOptions.SslCaLocation,
        //        saslMechanism: kafkaOptions.SaslMechanism,
        //        securityProtocol: kafkaOptions.SecurityProtocol
        //     ).MinimumLevel.ControlledBy(LoggingLevelSwitch);

        //}

        //if (elkOptions.Enabled)
        //{
        //    loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elkOptions.Url))
        //    {
        //        AutoRegisterTemplate = true,
        //        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,

        //        IndexFormat = string.IsNullOrWhiteSpace(elkOptions.IndexFormat) ? "logstash-{0:yyyy.MM.dd}" : elkOptions.IndexFormat,

        //        ModifyConnectionSettings = connectionConfiguration =>
        //            elkOptions.BasicAuthEnabled
        //                ? connectionConfiguration.BasicAuthentication(elkOptions.Username, elkOptions.Password) : connectionConfiguration

        //    }).MinimumLevel.ControlledBy(LoggingLevelSwitch);
        //}

        if (elkLogstashOptions.Enabled)
        {
            loggerConfiguration.WriteTo.ElasticEcsLogstash(
                requestUri: elkLogstashOptions.RequestUri,
                batchPostingLimit: elkLogstashOptions.BatchPostingLimit,
                queueLimit: elkLogstashOptions.QueueLimit,
                period: elkLogstashOptions.Period

             ).MinimumLevel.ControlledBy(LoggingLevelSwitch);

        }

        if (seqOptions.Enabled)
        {
            loggerConfiguration.WriteTo.Seq(seqOptions.Url, apiKey: seqOptions.ApiKey);
        }

        if (msSqlDBOptions.Enabled)
        {
            loggerConfiguration.WriteTo.MSSqlServer(
               connectionString: msSqlDBOptions.ConnectionString,
               sinkOptions: new MSSqlServerSinkOptions
               {
                   TableName = msSqlDBOptions.TableName,
                   SchemaName = msSqlDBOptions.SchemaName,
                   AutoCreateSqlTable = true
               },
               restrictedToMinimumLevel: LogEventLevel.Debug,
               formatProvider: null,
               columnOptions: null,
               logEventFormatter: null);
        }

        if (sendMailOptions.Enabled)
        {
            loggerConfiguration.WriteTo.Email(
                sendMailOptions.EmailFrom,
                sendMailOptions.EmailTo,
                sendMailOptions.MailServer,
                sendMailOptions.Port,
                MailKit.Security.SecureSocketOptions.Auto,
                credentials: new NetworkCredential
                {
                    UserName = sendMailOptions.UserName,
                    Password = sendMailOptions.Password,
                },
                sendMailOptions.Subject,
                body: null,
                formatProvider: null,
                restrictedToMinimumLevel: LogEventLevel.Error

            ).MinimumLevel.ControlledBy(LoggingLevelSwitch);
        }

        //if (mongoDBOptions.Enabled)
        //{
        //    loggerConfiguration.WriteTo.MongoDBCapped(
        //        databaseUrl: mongoDBOptions.ConnectionString + "/" + mongoDBOptions.Database,
        //        cappedMaxSizeMb: 1024,
        //        cappedMaxDocuments: 100000,
        //        batchPostingLimit: 100,
        //        collectionName: mongoDBOptions.CollectionName
        //     );
        //}

        //if (lokiOptions.Enabled)
        //{
        //    if (lokiOptions.LokiUsername is not null && lokiOptions.LokiPassword is not null)
        //    {
        //        var auth = new LokiCredentials
        //        {
        //            Login = lokiOptions.LokiUsername,
        //            Password = lokiOptions.LokiPassword
        //        };

        //        loggerConfiguration.WriteTo.GrafanaLoki(
        //            lokiOptions.Url,
        //            credentials: auth,
        //            batchPostingLimit: lokiOptions.BatchPostingLimit ?? 1000,
        //            queueLimit: lokiOptions.QueueLimit,
        //            period: lokiOptions.Period
        //         ).MinimumLevel.ControlledBy(LoggingLevelSwitch);
        //    }
        //    else
        //    {
        //        loggerConfiguration.WriteTo.GrafanaLoki(
        //            lokiOptions.Url,
        //            batchPostingLimit: lokiOptions.BatchPostingLimit ?? 1000,
        //            queueLimit: lokiOptions.QueueLimit,
        //            period: lokiOptions.Period
        //        ).MinimumLevel.ControlledBy(LoggingLevelSwitch); ;
        //    }
       // }
    }

    internal static LogEventLevel GetLogEventLevel(string level) => Enum.TryParse<LogEventLevel>(level, true, out var logLevel) ? logLevel : LogEventLevel.Information;

    public static IServiceCollection AddCorrelationContextLogging(this IServiceCollection builder)
    {
        builder.AddTransient<LoggingCorrelationMiddleware>();

        return builder;
    }

    public static IApplicationBuilder UserCorrelationContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<LoggingCorrelationMiddleware>();
        return app;
    }

    private static async Task LevelSwitch(HttpContext context)
    {
        var service = context.RequestServices.GetService<ILoggingService>();
        if (service is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("ILoggingService is not registered. Add UseButterflyLogging() to your Program.cs.");
            return;
        }

        var level = context.Request.Query["level"].ToString();

        if (string.IsNullOrEmpty(level))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid value for logging level.");
            return;
        }

        service.SetLoggingLevel(level);

        context.Response.StatusCode = StatusCodes.Status200OK;
    }

    // public static T BindOptions<T>(this IConfiguration configuration, string sectionName) where T : new()
    //   => BindOptions<T>(configuration.GetSection(sectionName));

    // public static T BindOptions<T>(this IConfigurationSection section) where T : new()
    // {
    //     var options = new T();
    //     section.Bind(options);
    //     return options;
    // }
    // public static TModel GetOptions<TModel>(this IConfiguration configuration, string sectionName)
    //   where TModel : new()
    // {
    //     var model = new TModel();
    //     configuration.GetSection(sectionName).Bind(model);
    //     return model;
    // }
}