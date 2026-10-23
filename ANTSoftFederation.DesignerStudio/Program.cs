
using SorteFederation.DesignerStudio.Services;
using SorteFederation.Logger;
using SorteFederation.Shared;
using SorteFederation.Shared.EndPoints;
using SorteFederation.Shared.TemplateCatalog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRabbitMQClient("messaging");

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ITemplateCatalogEventBus, RabbitMqTemplateCatalogEventBus>();
builder.Services.AddHostedService<PublishedTemplateCatalogReplayService>();
builder.Host.UseANTLogging();

var jwtKey = builder.Configuration["Auth:Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Auth:Jwt:Key must be configured with at least 32 bytes.");

var designerStudioConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Filename=./Contents/Data/sorteDesignStudio.db";
builder.Services.AddDbContext<DesignerStudioDbContext>(options => options.UseSqlite(designerStudioConnectionString));
builder.Services.AddAntiforgery();

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
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DesignerStudioAccess", policy =>
        policy.RequireAuthenticatedUser().RequireRole("SystemAdmin", "Designer"));
    options.AddPolicy("TemplateReview", policy =>
        policy.RequireAuthenticatedUser().RequireRole("SystemAdmin"));
});

builder.Services.ConfigureCorsPolicy();

/*var connectionString = builder.Configuration.GetConnectionString("SorteDesignerStudioDb2026")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException(
        "Configure an Aspire 'SorteDesignerStudioDb2026' connection or a local 'DefaultConnection'/'PostgreSQL' connection string.");

builder.Services.AddDbContext<DesignerStudioDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "designer_studio")));
*/

builder.Services.AddHttpClient<FigmaImportService>(client => client.Timeout = TimeSpan.FromSeconds(45));

var packageStoragePath = builder.Configuration["DESIGNER_TEMPLATE_PACKAGES_PATH"]
    ?? builder.Configuration["TemplatePackages:StoragePath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "TemplatePackages");

builder.Services.AddSingleton<ITemplatePackageStorage>(new FileSystemTemplatePackageStorage(packageStoragePath));

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    options.MultipartBodyLengthLimit = TemplatePackageEndpoints.MaximumPackageBytes + 1024 * 1024);

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = TemplatePackageEndpoints.MaximumPackageBytes + 1024 * 1024);

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
var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
    {
        context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        return;
    }

    var status = exception switch
    {
        ArgumentException => StatusCodes.Status400BadRequest,
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        InvalidOperationException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
    context.Response.StatusCode = status;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        Status = status,
        Title = status switch
        {
            StatusCodes.Status400BadRequest => "Template package validation failed.",
            StatusCodes.Status403Forbidden => "You are not authorized to perform this operation.",
            StatusCodes.Status404NotFound => "The requested template was not found.",
            StatusCodes.Status409Conflict => "The request conflicts with the current template state.",
            _ => "An unexpected Designer Studio error occurred."
        },
        Detail = app.Environment.IsDevelopment() ? exception?.Message : null,
        Instance = context.Request.Path
    });
}));

app.UsePathBase("/designer-studio-app");
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.UseCors("SortePolicy");

if (app.Environment.IsDevelopment()) app.MapOpenApi().RequireAuthorization("DesignerStudioAccess");

app.MapScalarApiReference(o =>
{
    o.DarkMode = true;
    o.WithTitle("Sorte Design Studio Docs");
    o.WithTitle("Sorte Design Studio Documentation")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

    o.Authentication = new ScalarAuthenticationOptions
    {

    };
}).RequireAuthorization("DesignerStudioAccess");

var api = app.MapGroup("/api/v1").RequireAuthorization();
TemplatePackageEndpoints.Map(api);
app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DesignerStudioDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
