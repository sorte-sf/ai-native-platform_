namespace SorteFederation.Portal;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddServiceDiscovery();
        builder.AddRabbitMQClient("messaging");
        builder.Services.AddRealtimeEventBus("portal-api");

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, _) =>
            {
                document.Info = new()
                {
                    Title = "Portal Documentations",
                    Version = "v1",
                    Contact = new()
                    {
                        Name = "API Support"
                    }
                };
                return Task.CompletedTask;
            });
        });

        // WebApplicationBuilder already loads appsettings, environment variables,
        // and User Secrets. Do not reload these providers here.
         
        builder.Services.AddRateLimiting()
               .AddDatabase<ANTSoftPortalDbContext>(builder.Configuration)
               .AddHttpContextAccessor()
               .AddSingleton(typeof(ISecurityService), typeof(SecurityService))
               .AddSingleton(typeof(IDbInitializerService), typeof(DbInitializerService))
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

        builder.Services.AddSingleton<DatabaseService>();
        builder.EnrichNpgsqlDbContext<ANTSoftPortalDbContext>();

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddEndPoints(Assembly.GetExecutingAssembly());

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

        builder.Services.ConfigureCorsPolicy();

        // TokenService still binds the legacy JwtSettings section. Accept that same
        // configuration here so Portal-issued tokens and Portal's validator agree.
        var jwtKey = builder.Configuration["Auth:Jwt:Key"] ?? builder.Configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
            throw new InvalidOperationException("Auth:Jwt:Key must be configured with at least 32 characters.");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Auth:Jwt:Issuer"] ?? builder.Configuration["JwtSettings:Issuer"] ?? "sorte.portal",
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Auth:Jwt:Audience"] ?? builder.Configuration["JwtSettings:Audience"] ?? "antsoft",
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
        builder.Services.AddAuthorization();

        builder.Services.AddScoped<TokenService>();
        builder.Services.AddHttpClient<TcmbExchangeRateImporter>();
        builder.Services.AddHostedService<TcmbExchangeRateBackgroundService>();

        // Register application services
        builder.Services.AddScoped<IApplicationIntegratorKeyService, ApplicationIntegratorKeyService>();
        builder.Services.AddScoped<IApplicationEmbeddingService, ApplicationEmbeddingService>();
        builder.Services.AddScoped<IApplicationDocService, ApplicationDocService>();
        builder.Services.AddScoped<IApplicationClaimService, ApplicationClaimService>();
        builder.Services.AddScoped<IApplicationModuleIssueMessageService, ApplicationModuleIssueMessageService>();
        builder.Services.AddScoped<IApplicationModuleIssueService, ApplicationModuleIssueService>();
        builder.Services.AddScoped<IApplicationModuleService, ApplicationModuleService>();
        builder.Services.AddScoped<IApplicationMenuService, ApplicationMenuService>();
        builder.Services.AddScoped<ICompanyAppVersionService, CompanyAppVersionService>();
        builder.Services.AddScoped<ICompanyApplicationLicenceService, CompanyApplicationLicenceService>();
        builder.Services.AddScoped<ICompanyApplicationService, CompanyApplicationService>();
        builder.Services.AddScoped<ICompanyService, CompanyService>();
        builder.Services.AddScoped<IProjectApplicationService, ProjectApplicationService>();
        builder.Services.AddScoped<IUserApplicationChatService, UserApplicationChatService>();

        var defaultConnection = builder.Configuration.GetConnectionString("SortePlatformDb2027")
            ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");

        builder.Services.AddSingleton<Npgsql.NpgsqlDataSource>(_ =>
        {
            var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(defaultConnection);
            dataSourceBuilder.UseVector();
            return dataSourceBuilder.Build();
        });

        builder.Services
             .AddSignalR(options => options.EnableDetailedErrors = true)
           //  .AddMessagePackProtocol()
             .AddJsonProtocol(options =>
             {
                 //  options.PayloadSerializerOptions.WriteIndented = true;
                 options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                 options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                 options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());

             });

        return builder;

    }


    public static IServiceCollection AddDatabase<T>(this IServiceCollection services, IConfiguration configuration)
         where T : DbContext
    {
        var connectionStr = configuration.GetConnectionString("SortePlatformDb2027")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionStr))
        {
            throw new InvalidOperationException("Database connection string is missing. Configure ConnectionStrings:DefaultConnection.");
        }

        services.AddDbContext<T>(options =>
        {
            options.UseNpgsql(connectionStr, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
            });

            // Startup applies migrations against environments that may have been built from a
            // semantically identical snapshot by a different EF/Npgsql patch version. Keep
            // migration execution enabled while preventing that snapshot-only warning from
            // stopping the API before it can serve requests.
            options.ConfigureWarnings(x => x
                .Ignore(RelationalEventId.AmbientTransactionWarning)
                .Ignore(RelationalEventId.PendingModelChangesWarning));

        });
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return services;
    }

}

