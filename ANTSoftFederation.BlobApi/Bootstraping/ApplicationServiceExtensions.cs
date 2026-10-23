
namespace SorteFederation.BlobApi.Bootstraping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddRabbitMQClient("messaging");
        builder.Services.AddRealtimeEventBus("blob-api");
        builder.Services.AddProblemDetails();

        var jwtKey = builder.Configuration["Auth:Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
            throw new InvalidOperationException("Auth:Jwt:Key must be configured with at least 32 characters.");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Auth:Jwt:Issuer"] ?? "sorte.portal",
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Auth:Jwt:Audience"] ?? "antsoft",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                RoleClaimType = ClaimTypes.Role
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (string.IsNullOrEmpty(context.Token) && context.Request.Cookies.TryGetValue(SsoTokenCookie.Name, out var token))
                        context.Token = token;
                    return Task.CompletedTask;
                }
            };
        });
        builder.Services.ConfigureCorsPolicy();

        builder.Services.AddAuthorization();
        builder.Services.AddScoped<ApplicationDocumentService>();

        builder.Services.AddServiceDiscovery();

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, _) =>
            {
                document.Info = new()
                {
                    Title = "Sorte Blob Documentations",
                    Version = "v1",
                    Description = """
                Modern API for managing Blob API.
                Supports JSON and XML responses.
                Rate limited to 1000 requests per hour.
                """,
                    Contact = new()
                    {
                        Name = "API Support"
                    }
                };
                return Task.CompletedTask;
            });
        });

        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();

        builder.Services
               .AddDatabase<BlobDbContext>(builder.Configuration)
               .AddHttpContextAccessor()
               .AddMemoryCache()
               .Configure<FormOptions>(o =>
               {
                   o.ValueLengthLimit = int.MaxValue;
                   o.MultipartBodyLengthLimit = int.MaxValue;
                   o.MemoryBufferThreshold = int.MaxValue;

               })
               .ConfigureHttpJsonOptions(options =>
               {
                   options.SerializerOptions.WriteIndented = true;
                   options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                   options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                   options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                   options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
               });

        builder.EnrichNpgsqlDbContext<BlobDbContext>(); 

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddApiVersioning(opts =>
        {
            opts.ReportApiVersions = true;
            opts.DefaultApiVersion = new ApiVersion(1.0);
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(opts =>
        {
            opts.GroupNameFormat = "'v'VVV";
            opts.SubstituteApiVersionInUrl = true;
        });
         
        return builder;
    }

    public static IServiceCollection AddDatabase<T>(this IServiceCollection services, IConfiguration configuration)
         where T : DbContext
    {
        var connectionStr = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionStr))
        {
            throw new InvalidOperationException("Database connection string is missing. Configure an Aspire PostgreSQL connection (DefaultConnection).");
        }

        services.AddDbContext<T>(options =>
        {
            options.UseNpgsql(connectionStr, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
            });

            options.ConfigureWarnings(x => x
                .Ignore(RelationalEventId.AmbientTransactionWarning)
                .Ignore(RelationalEventId.PendingModelChangesWarning));

        });
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return services;
    }
}

