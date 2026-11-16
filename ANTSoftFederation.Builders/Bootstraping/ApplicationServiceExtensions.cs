
using Microsoft.AspNetCore.Authorization;

namespace SorteFederation.ANTApplications.Bootstraping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var encryptionKey = builder.Configuration["Encryption:Key"];
        if (string.IsNullOrWhiteSpace(encryptionKey) || Encoding.UTF8.GetByteCount(encryptionKey) is not (16 or 24 or 32))
            throw new InvalidOperationException("Encryption:Key must be configured as a 16, 24, or 32-byte AES key.");
        builder.Services.AddSingleton<IEncryptionProvider>(new GenerateEncryptionProvider(encryptionKey));

        builder.Services.AddServiceDiscovery();
        builder.AddRabbitMQClient("messaging");
        builder.Services.AddRealtimeEventBus("builders-api");

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, _) =>
            {
                document.Info = new()
                {
                    Title = "Sorte Designer Studio",
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


        var jwtKey = builder.Configuration["Auth:Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
            throw new InvalidOperationException("Auth:Jwt:Key must be configured with at least 32 characters.");

        builder.Services.ConfigureCorsPolicy();

        // var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200", "http://localhost:4201"];

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("builders-api", context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.FindFirst("tenant_id")?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 300,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

            options.AddPolicy("builders-ai", context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.FindFirst("tenant_id")?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
        });

        builder.Services.AddHttpClient("builders-connectors", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        builder.Services.AddHttpClient("blob-documents", client => client.Timeout = TimeSpan.FromMinutes(2));

        builder.Services.AddDbContext<BuilderDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("SortePlatformBuilderDb2027")
                ?? builder.Configuration.GetConnectionString("builders-db")
                ?? builder.Configuration.GetConnectionString("ANTPortalDb") ?? throw new InvalidOperationException("Aspire PostgreSQL connection 'ANTPortalDb' is required."),
                npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        builder.Services.AddDbContext<TenantRuntimeDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("SortePlatformBuilderDb2027")
                ?? builder.Configuration.GetConnectionString("builders-db")
                ?? builder.Configuration.GetConnectionString("ANTPortalDb") ?? throw new InvalidOperationException("Aspire PostgreSQL connection 'ANTPortalDb' is required."),
                npgsql => npgsql.UseVector()
                    .MigrationsAssembly(typeof(TenantRuntimeDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "builders_runtime")
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        builder.Services
               .AddDatabase<BuilderDbContext>(builder.Configuration)
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

        builder.EnrichNpgsqlDbContext<BuilderDbContext>();

        //builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

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

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
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
            })
            .AddJwtBearer("BuilderAppUser", options =>
            {
                var builderUsers = builder.Configuration.GetSection("Auth:BuilderUsers");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builderUsers["Issuer"] ?? "sorte.builder.users",
                    ValidateAudience = true,
                    ValidAudience = builderUsers["Audience"] ?? "antsoft.builder.apps",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var principal = context.Principal;
                    var tenantClaim = principal?.FindFirstValue("tenant_id");
                    var userClaim = principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("sub");
                    if (!Guid.TryParse(tenantClaim, out var tenantId) || !Guid.TryParse(userClaim, out var userId) ||
                        !int.TryParse(principal?.FindFirstValue("security_version"), out var tokenVersion))
                    {
                        context.Fail("The application-user token is invalid.");
                        return;
                    }

                    var db = context.HttpContext.RequestServices.GetRequiredService<BuilderDbContext>();
                    var account = await db.ApplicationUsers.IgnoreQueryFilters().AsNoTracking()
                        .Where(user => user.Id == userId && user.TenantId == tenantId && user.RecordState == RecordState.Active)
                        .Select(user => new { user.IsActive, user.SecurityVersion })
                        .SingleOrDefaultAsync(context.HttpContext.RequestAborted);
                    if (account is null || !account.IsActive || account.SecurityVersion != tokenVersion)
                        context.Fail("This application-user session is no longer active.");
                }
            };
            });
        builder.Services.AddAuthentication().AddJwtBearer("BuilderWorkspace", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Auth:BuilderWorkspace:Issuer"] ?? "sorte.builder.workspace",
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Auth:BuilderWorkspace:Audience"] ?? "antsoft.builder.workspace",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                RoleClaimType = ClaimTypes.Role
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var principal = context.Principal;
                    if (!Guid.TryParse(principal?.FindFirstValue("tenant_id"), out var tenantId) ||
                        !Guid.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("sub"), out var userId) ||
                        !Guid.TryParse(principal?.FindFirstValue("builder_workspace_access_id"), out var accessId) ||
                        !int.TryParse(principal?.FindFirstValue("builder_workspace_security_version"), out var version))
                    {
                        context.Fail("The Builder workspace token is invalid.");
                        return;
                    }
                    var db = context.HttpContext.RequestServices.GetRequiredService<BuilderDbContext>();
                    var valid = await db.WorkspaceAccess.IgnoreQueryFilters().AsNoTracking()
                        .AnyAsync(x => x.Id == accessId && x.TenantId == tenantId && x.UserId == userId && x.IsActive &&
                            x.RecordState == RecordState.Active && x.SecurityVersion == version &&
                            x.User.IsActive && x.User.RecordState == RecordState.Active,
                            context.HttpContext.RequestAborted);
                    if (!valid) context.Fail("Builder workspace access has been revoked.");
                }
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, "BuilderWorkspace")
                .RequireAuthenticatedUser().Build();
            options.AddPolicy("BuilderAppUser", policy =>
            {
                policy.AddAuthenticationSchemes("BuilderAppUser");
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("builder_user", "true");
            });
        });

        builder.Services.AddScoped<BuilderMetadataService>();
        builder.Services.AddScoped<DesignerStudioCatalogSyncService>();
        builder.Services.AddScoped<TemplateUpdateService>();
        builder.Services.AddScoped<PublishedApplicationDraftService>();
        builder.Services.AddScoped<IRealtimeEventHandler, PortalCompanyTenantSyncHandler>();
        builder.Services.AddScoped<FormDesignerService>();
        builder.Services.AddScoped<BuilderAuthorizationService>();
        builder.Services.AddScoped<BuilderApplicationUserService>();
        builder.Services.AddScoped<BuilderWorkspaceAuthService>();
        builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<BuilderApplicationUser>, Microsoft.AspNetCore.Identity.PasswordHasher<BuilderApplicationUser>>();
        builder.Services.AddScoped<PlatformRuntimeService>();
        builder.Services.AddScoped<AuditService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<TenantRuntimeContextFactory>();
        builder.Services.AddScoped<TenantRecordService>();
        builder.Services.AddScoped<ConnectorExecutionService>();
        builder.Services.AddSingleton<ISecretResolver, VaultOrEnvironmentSecretResolver>();
        builder.Services.AddScoped<FormRuntimeService>();
        builder.Services.AddSingleton<PlatformEventQueue>();
        builder.Services.AddHostedService<PlatformEventWorker>();
        
        builder.Services.AddHostedService<WorkflowExecutionWorker>();
        builder.Services.AddScoped<DynamicQueryService>();
        builder.Services.AddScoped<TenantWorkflowRuntimeService>();
        builder.Services.AddScoped<PurchaseApprovalRuntimeService>();
        builder.Services.AddScoped<PurchaseOrganizationService>();
        builder.Services.AddScoped<LoadEmailSettings>();
        builder.Services.AddScoped<IMailKitService, MailKitService>();
        builder.Services.AddScoped<TenantRelationshipService>();
        builder.Services.AddScoped<KnowledgeService>();
        builder.Services.AddScoped<KnowledgeEmbeddingService>();
        builder.Services.AddScoped<SolutionDocumentService>();
        builder.Services.AddScoped<SupportDeskService>();
        builder.Services.AddScoped<AiGatewayService>();
        builder.Services.AddScoped<TenantAiPolicyService>();
        builder.Services.AddScoped<AiUsageMeter>();
        builder.Services.AddScoped<AiInferenceService>();
        builder.Services.AddScoped<PublishedApplicationAiService>();
        builder.Services.AddScoped<FeatureFlagService>();
        builder.Services.AddScoped<RulesRuntimeService>();
        builder.Services.AddScoped<RuleDefinitionService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<PlatformActionDispatcher>();
        builder.Services.AddScoped<IScheduledJobActionDispatcher, ScheduledJobActionDispatcher>();
        
        //builder.Services.AddScoped<ScheduledJobTimeZoneService>();
        //builder.Services.Configure<ScheduledJobRetryOptions>(builder.Configuration.GetSection("ScheduledJobs:Retry"));
        //builder.Services.AddHostedService<ScheduledJobTickerReconciler>();

        builder.Services.AddScoped<IScheduledJobActionHandler, PlatformScheduledActionHandler>();
        builder.Services.AddScoped<IScheduledJobActionHandler, ScheduledNotificationActionHandler>();
        builder.Services.AddScoped<IScheduledJobActionHandler, ScheduledWorkflowActionHandler>();
        builder.Services.AddScoped<IScheduledJobActionHandler, ScheduledReportActionHandler>();
        builder.Services.AddScoped<ScheduledMailService>();
        builder.Services.AddSingleton<ScheduledReportArtifactStore>();
        builder.Services.AddScoped<IScheduledJobActionHandler, ScheduledEmailActionHandler>();
        builder.Services.Configure<ScheduledEmailOptions>(builder.Configuration.GetSection("ScheduledJobs:Mail"));
        builder.Services.Configure<ScheduledApiOptions>(builder.Configuration.GetSection("ScheduledJobs:Api"));

        builder.Services.AddHttpClient<ScheduledApiActionHandler>(client => client.Timeout = TimeSpan.FromSeconds(30))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        builder.Services.AddScoped<IScheduledJobActionHandler>(provider =>
            provider.GetRequiredService<ScheduledApiActionHandler>());

        /*  var defaultConnection = builder.Configuration.GetConnectionString("SorteBuilderDb2026")
              ?? builder.Configuration.GetConnectionString("builders-db")
              ?? builder.Configuration.GetConnectionString("ANTPortalDb")
              ?? builder.Configuration.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("No Aspire PostgreSQL connection was provided. Expected SorteBuilderDb2026, builders-db, ANTPortalDb, or DefaultConnection.");

          builder.Services.AddSingleton<Npgsql.NpgsqlDataSource>(_ =>
          {
              var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(defaultConnection);
              dataSourceBuilder.UseVector();
              return dataSourceBuilder.Build();
          });*/

       /* builder.Services.AddTickerQ(options =>
       {
           options.SetExceptionHandler<TickerExceptionHandler>();
           options.AddOperationalStore(efOptions =>
             {
             efOptions.UseApplicationDbContext<BuilderDbContext>(ConfigurationType.IgnoreModelCustomizer);
             efOptions.SetSchema("ticker");
         });

           options.AddDashboard(o =>
             {
             o.SetBasePath("/sorte-q");

             o.SetCorsPolicy(cors =>
             {
                 cors
                     .AllowAnyOrigin()
                     .AllowAnyMethod()
                     .WithHeaders("Authorization", "Content-Type")
                     .SetIsOriginAllowed(origin => true);
             });

             o.WithBasicAuth("admin", "ChangeMe123!");
         });

       });*/

        builder.Services
             .AddSignalR(options => options.EnableDetailedErrors = true)
             // .AddMessagePackProtocol()
             .AddJsonProtocol(options =>
             {
                 //  options.PayloadSerializerOptions.WriteIndented = true;
                 options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                 options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                 options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());

             });

        builder.AddKeyedOllamaApiClient("chat", "ollama-chat")
               .AddKeyedChatClient();

        builder.AddKeyedOllamaApiClient("embeddings", "ollama-embeddings")
               .AddEmbeddingGenerator();

        // The Ollama integration creates these named clients. Local inference can
        // exceed the shared 30-second resilience budget; never retry generation.
        // Keep service discovery/telemetry, and leave other HTTP clients unchanged.
        foreach (var connectionName in new[] { "ollama-chat", "ollama-embeddings" })
        {
#pragma warning disable EXTEXP0001 // Supported per-client opt-out from the shared resilience pipeline.
            builder.Services.AddHttpClient($"{connectionName}_httpClient", client =>
                client.Timeout = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(10))
                .RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001
        }

        return builder;

    }

    public static IServiceCollection AddDatabase<T>(this IServiceCollection services, IConfiguration configuration)
         where T : DbContext
    {
        var connectionStr = configuration.GetConnectionString("SortePlatformBuilderDb2027")
            ?? configuration.GetConnectionString("builders-db") 
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionStr))
        {
            throw new InvalidOperationException("Database connection string is missing. Configure an Aspire PostgreSQL connection (SorteBuilderDb2026, builders-db, or ANTPortalDb).");
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

