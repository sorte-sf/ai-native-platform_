namespace SorteFederation.LoadBalancing;

public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {

        builder.Services.AddProblemDetails();

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        builder.Services.AddCors(opt => opt.AddPolicy("SortePolicy", builder =>
        {
            builder
                   .AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        }));

        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();

        builder.Services
               .AddReverseProxy()
               .ConfigureHttpClient((context, handler) => handler.AllowAutoRedirect = true)
               .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
               .AddTransforms(transforms =>
               {
                   transforms.AddRequestTransform(context =>
                   {
                       if (context.HttpContext.Request.Headers.TryGetValue("api-version", out var apiVersion))
                       {
                           context.ProxyRequest.Headers.Add("X-Api-Version", apiVersion.ToString());
                       }

                       if (context.HttpContext.Request.Headers.TryGetValue("Origin", out var origin))
                       {
                           context.ProxyRequest.Headers.Add("Origin", origin.ToString());
                       }

                       return ValueTask.CompletedTask;
                   });
               })
               .AddServiceDiscoveryDestinationResolver()
               ;

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings configuration is required.");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };

        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("SortePolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        return builder;
    }
}
