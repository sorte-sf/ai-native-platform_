
using SorteFederation.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// builder.Services.AddLettuceEncrypt();

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

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
       .AddEnvironmentVariables();

var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("SortePolicy", policy => policy
    .SetIsOriginAllowed(origin =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
        ((builder.Environment.IsDevelopment() && uri.IsLoopback) || allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)))
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()));

builder.Services
       .AddReverseProxy()
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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Auth:Jwt");
    var issuer = jwtSettings["Issuer"] ?? builder.Configuration["JwtSettings:Issuer"]
        ?? throw new InvalidOperationException("Auth:Jwt:Issuer configuration is required.");
    var audience = jwtSettings["Audience"] ?? builder.Configuration["JwtSettings:Audience"]
        ?? throw new InvalidOperationException("Auth:Jwt:Audience configuration is required.");
    var secretKey = jwtSettings["Key"] ?? builder.Configuration["JwtSettings:SecretKey"]
        ?? throw new InvalidOperationException("Auth:Jwt:Key configuration is required.");
    if (Encoding.UTF8.GetByteCount(secretKey) < 32)
        throw new InvalidOperationException("Auth:Jwt:Key must be configured with at least 32 bytes.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.FromMinutes(1),
        RoleClaimType = ClaimTypes.Role
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies[SsoTokenCookie.Name];
            if (string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/realtime"))
                token = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token))
                context.Token = token;
            return Task.CompletedTask;
        }
    };

}).AddScheme<GatewayBasicAuthenticationOptions, GatewayBasicAuthenticationHandler>("GatewayBasic", options =>
{
    var section = builder.Configuration.GetSection("GatewayBasicAuth");
    options.Username = section["Username"] ?? string.Empty;
    options.Password = section["Password"] ?? string.Empty;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SortePolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

var app = builder.Build();
 
var figgleText = FiggleFonts.Standard.Render(builder.Configuration.GetSection("app:name").Value ?? "Sorte-Gateway");
app.MapGet("/", ctx => ctx.Response.WriteAsync(figgleText));

app.UseAuthentication();
app.UseWebSockets();
app.UseRouting();

app.UseCors("SortePolicy");

app.UseAuthorization();
 
app.UseMiddleware<GatewayTokenValidationMiddleware>();

app.MapReverseProxy();

app.MapDefaultEndpoints();

app.Run();
 
