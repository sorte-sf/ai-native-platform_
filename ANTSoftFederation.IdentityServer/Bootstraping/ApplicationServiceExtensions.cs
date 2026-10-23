
namespace ANTSoftFederation.IdentityServer.Bootstraping;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddRabbitMQClient("messaging");
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddServiceDiscovery();
        builder.Services.AddRateLimiting();

        builder.Services.AddSingleton<DatabaseService>()
               .AddDbContext<ANTSoftIdentityDbContext>((serviceProvider, options) =>
               {
                   var databaseService = serviceProvider.GetRequiredService<DatabaseService>();
                   databaseService.ConfigureDatabase(options);
               });

        builder.EnrichNpgsqlDbContext<ANTSoftIdentityDbContext>();

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, _) =>
            {
                document.Info = new()
                {
                    Title = "ANTSoft Identity-Server API Docs",
                    Version = "v1",
                    Description = """
                Modern API for managing Identity API.
                Supports JSON and XML responses.
                Rate limited to 1000 requests per hour.
                """,
                    Contact = new()
                    {
                        Name = "API Support",
                        Email = "identity@antsoft.com.tr"
                    }
                };
                return Task.CompletedTask;
            });
        });

        builder.Services.AddCors(opt => opt.AddPolicy("ANTFedPolicy", builder =>
        {
            builder
                   .AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        }));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddMemoryCache();
         
        builder.Services.AddResponseCaching();

        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        builder.Services.Configure<FormOptions>(o =>
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

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var key = builder.Configuration.GetSection("Authentication:jwt:secretkey").Value;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Authentication:jwt:issuer"],
                ValidAudience = builder.Configuration["Authentication:jwt:audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Authentication:jwt:secretkey"]))                 
            };

        })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = "your-google-client-id";
            options.ClientSecret = "your-google-client-secret";
        })
        .AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
        {
            options.AppId = "your-facebook-app-id";
            options.AppSecret = "your-facebook-app-secret";
        })
        .AddTwitter(TwitterDefaults.AuthenticationScheme, options =>
        {
            options.ConsumerKey = "your-twitter-consumer-key";
            options.ConsumerSecret = "your-twitter-consumer-secret";
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.GetClaimsFromUserInfoEndpoint = true;

            var callbackPath = builder.Configuration["Authentication:Google:CallbackPath"];

            if (string.IsNullOrEmpty(callbackPath))
            {
                throw new ArgumentException("The 'CallbackPath' option must be provided.", nameof(options.CallbackPath));
            }
            options.CallbackPath = new PathString(callbackPath);

            options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
            options.ResponseMode = OpenIdConnectResponseMode.FormPost;

            options.Authority = "https://accounts.google.com";
            options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            options.ClientSecret = builder.Configuration["Authentication:Google:Secret"];

            options.ResponseType = OpenIdConnectResponseType.Code;
            options.UsePkce = true;
            options.SaveTokens = true;

            options.Scope.Add("openid");
            options.Scope.Add("email");
            options.Scope.Add("phone");
            options.Scope.Add("profile");

            options.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = async context =>
                {
                    var tokenStore = context.HttpContext.RequestServices.GetRequiredService<ITokenStorage>();
                    var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
                    var accessToken = context.SecurityToken as JwtSecurityToken;
                    if (accessToken != null)
                    {
                        tokenStore.StoreToken(userId, accessToken.RawData);
                    }
                },

                OnRedirectToIdentityProviderForSignOut = context =>
                {
                    context.Response.Redirect(builder.Configuration["Authentication:Google:RedirectOnSignOut"]);
                    context.HandleResponse();
                    return Task.CompletedTask;
                },

                OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/error");
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        })
        .AddScheme<AuthenticationSchemeOptions, ANTSoftFederation.IdentityServer.Middlewares.BasicAuthenticationHandler>("Basic", null);

        builder.Services.AddAuthorization();

        // Register all entity services

        builder.Services.AddSingleton(typeof(ISecurityService), typeof(SecurityService));
        builder.Services.AddSingleton(typeof(IDbInitializerService), typeof(DbInitializerService));
        builder.Services.AddSingleton<IMailKitService, MailKitService>();
        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.AddApiVersioning(opts =>
        {
            opts.ReportApiVersions = true;
            opts.DefaultApiVersion = new ApiVersion(1.0);
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(q =>
        {
            q.GroupNameFormat = "'v'VVV";
            q.SubstituteApiVersionInUrl = true;

        });


        builder.Services.AddScoped<BaseService>();

        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IClientService, ClientService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IClientScopeService, ClientScopeService>();
        builder.Services.AddScoped<IGroupService, GroupService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddScoped<IIdentityProviderService, IdentityProviderService>();
        builder.Services.AddScoped<IProtocolMapperService, ProtocolMapperService>();
        builder.Services.AddScoped<IUserProjectService, UserProjectService>();
        builder.Services.AddScoped<IProjectGroupService, ProjectGroupService>();
        builder.Services.AddScoped<IGroupRoleService, GroupRoleService>();
        builder.Services.AddScoped<IUserRoleService, UserRoleService>();
        builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
        builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();
        builder.Services.AddScoped<IUserGroupService, UserGroupService>();
        builder.Services.AddScoped<IClientRoleService, ClientRoleService>();
        builder.Services.AddScoped<IGroupPermissionService, GroupPermissionService>();
        builder.Services.AddScoped<IGroupAttributeService, GroupAttributeService>();
        builder.Services.AddScoped<IUserAttributeService, UserAttributeService>();
        builder.Services.AddScoped<IIdentityProviderConfigService, IdentityProviderConfigService>();
        builder.Services.AddScoped<IProtocolMapperConfigService, ProtocolMapperConfigService>();
        builder.Services.AddScoped<IUserSessionService, UserSessionService>();

        return builder;
    }

}
