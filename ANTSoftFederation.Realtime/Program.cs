
using Figgle.Fonts;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddRabbitMQClient("messaging");

var jwtKey = builder.Configuration["Auth:Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Auth:Jwt:Key must be configured with at least 32 characters.");

builder.Services.AddSingleton<IRealtimeEventBus, RabbitMqRealtimeEventBus>();
builder.Services.AddHostedService<RealtimeEventForwarder>();
builder.Services.AddSignalR();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Auth:Jwt:Issuer"] ?? "sorte.portal",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Auth:Jwt:Audience"] ?? "sorte",
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
            var accessToken = context.Request.Cookies[SsoTokenCookie.Name];
            if (string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/realtime/hub"))
                accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseWebSockets();

app.UseAuthentication();

app.UseAuthorization();

app.MapDefaultEndpoints();

var figgleText = FiggleFonts.Standard.Render(builder.Configuration.GetSection("app:name").Value ?? "Sorte RealTime API");
app.MapGet("/", ctx => ctx.Response.WriteAsync(figgleText));

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapHub<RealtimeHub>("/realtime/hub").RequireAuthorization();

app.Run();
