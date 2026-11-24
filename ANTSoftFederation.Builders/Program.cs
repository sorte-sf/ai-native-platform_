
using SorteFederation.Builders;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.AddApplicationServices();
builder.Services.AddSingleton<PlatformPluginRegistry>();
builder.Services.AddProblemDetails();
builder.Services.AddEndPoints(typeof(Program).Assembly);

builder.Host.UseANTLogging();
builder.Services.AddHttpClient("builders-connectors", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

builder.Services.AddSingleton<ITemplateCatalogEventBus, RabbitMqTemplateCatalogEventBus>();
builder.Services.AddHostedService<DesignerStudioTemplateReleaseSubscriber>();

//builder.Services.AddHealthChecks().AddDbContextCheck<BuilderDbContext>();

/*
var jwtKey = builder.Configuration["Auth:Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Auth:Jwt:Key must be configured with at least 32 characters.");


var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200", "http://localhost:4201"];
 
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

builder.Services.AddDbContext<BuilderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SortePlatformBuilderDb2027")
        ?? builder.Configuration.GetConnectionString("builders-db")
        ?? builder.Configuration.GetConnectionString("ANTPortalDb") ?? throw new InvalidOperationException("Aspire PostgreSQL connection 'ANTPortalDb' is required."),
        npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddDbContext<TenantRuntimeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SortePlatformBuilderDb2027")
        ?? builder.Configuration.GetConnectionString("builders-db")
        ?? builder.Configuration.GetConnectionString("ANTPortalDb") ?? throw new InvalidOperationException("Aspire PostgreSQL connection 'ANTPortalDb' is required."),
        npgsql => npgsql.MigrationsAssembly(typeof(TenantRuntimeDbContext).Assembly.FullName)
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
*/

var spaRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
if (!File.Exists(Path.Combine(spaRoot, "index.html")))
{
    var outputSpaRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
    if (File.Exists(Path.Combine(outputSpaRoot, "index.html")))
        spaRoot = outputSpaRoot;
}

builder.WebHost.UseWebRoot(spaRoot);

//builder.Services.AddDbContext<BlobDbContext>(x => x.UseSqlite("DataSource=./LocalData/JobRunners.db")).AddAntiforgery();


if (builder.Environment.IsDevelopment())
{
    var hangfireDirectory = Path.Combine(builder.Environment.ContentRootPath, "LocalData");
    Directory.CreateDirectory(hangfireDirectory);
    var hangfireDatabase = Path.Combine(hangfireDirectory, "JobRunners.db");

    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSQLiteStorage($"Data Source={hangfireDatabase};"));

    builder.Services.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(1));
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    var exception = feature?.Error;
    if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
    {
        context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        return;
    }
    var status = exception switch
    {
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        ArgumentException => StatusCodes.Status400BadRequest,
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
            StatusCodes.Status400BadRequest => "Request validation failed.",
            StatusCodes.Status403Forbidden => "You are not authorized to perform this operation.",
            StatusCodes.Status404NotFound => "The requested resource was not found.",
            StatusCodes.Status409Conflict => "The request conflicts with the current resource state.",
            _ => "An unexpected server error occurred."
        },
        Detail = app.Environment.IsDevelopment() ? exception?.Message : null,
        Instance = context.Request.Path
    });
}));

/*app.MapHub<AntChatHub>("/antsoftChatHub", options =>
{
    options.AllowStatefulReconnects = true;
    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling | HttpTransportType.ServerSentEvents;
});*/

app.UseHttpsRedirection();

app.UseRouting();
app.UseStaticFiles();
app.UseCors("SortePolicy");

app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true &&
        Guid.TryParse(context.Request.RouteValues["applicationId"]?.ToString(), out var applicationId) &&
        !(HttpMethods.IsGet(context.Request.Method) && context.Request.Path.Value!.EndsWith("/published", StringComparison.OrdinalIgnoreCase)))
    {
        var db = context.RequestServices.GetRequiredService<BuilderDbContext>();
        var application = await db.Applications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == applicationId, context.RequestAborted);
        if (application is null) { context.Response.StatusCode = 404; return; }
        if (await db.ApplicationDeletionRequests.AsNoTracking().AnyAsync(x => x.ApplicationId == applicationId && x.Status == ApplicationDeletionRequestStatus.Pending, context.RequestAborted))
        {
            context.Response.StatusCode = 423;
            await context.Response.WriteAsync("This application is locked while its deletion request is pending approval.", context.RequestAborted);
            return;
        }
        var allowed = HttpMethods.IsGet(context.Request.Method)
            ? ApplicationEditingPolicy.CanView(context.User, application)
            : ApplicationEditingPolicy.CanEdit(context.User, application);
        if (ApplicationEditingPolicy.HasWorkspaceRole(context.User, "BuilderViewer") && !HttpMethods.IsGet(context.Request.Method))
            allowed = false;
        if (!allowed) { context.Response.StatusCode = 403; return; }
    }
    await next();
});

app.UseRateLimiter();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Correlation-Id"] = context.TraceIdentifier;
    await next();
});

await using (var scope = app.Services.CreateAsyncScope())
{
    var metadataDb = scope.ServiceProvider.GetRequiredService<BuilderDbContext>();
    await metadataDb.Database.MigrateAsync();
    await BuiltInScheduledJobTemplates.SeedAsync(metadataDb);
    await BuiltInCrmTemplate.SeedAsync(metadataDb);
    await BuiltInPurchaseTemplate.SeedAsync(metadataDb);
    var runtimeDb = scope.ServiceProvider.GetRequiredService<TenantRuntimeDbContext>();
    await runtimeDb.Database.MigrateAsync();

    //if (app.Environment.IsDevelopment())
    //    await DevelopmentSeed.SeedAsync(metadataDb);
}

// app.UseTickerQ();

app.MapScalarApiReference(o =>
{
    o.DarkMode = true;
    o.WithTitle("Sorte Design Studio API Docs");
    o.WithTitle("Sorte Design Studio API Documentation")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

    o.Authentication = new ScalarAuthenticationOptions
    {

    };
});

if (app.Environment.IsDevelopment())
{
    // Aspire polls /health frequently. Keep that liveness probe independent of
    // dependency-check cancellation; use /ready for the registered health checks.
    app.MapGet("/health", () => Results.Ok()).AllowAnonymous();
    app.MapGet("/alive", () => Results.Ok()).AllowAnonymous();
    app.MapHealthChecks("/ready").AllowAnonymous();
}

var endpoint = Environment.GetEnvironmentVariable("OLLAMA_LLAMA3_URI");
var modelName = Environment.GetEnvironmentVariable("OLLAMA_LLAMA3_MODEL");

/*app.MapPost("/chat", async (Microsoft.Extensions.AI.IChatClient chatClient, string prompt) =>
{
    var response = await chatClient. (prompt);
    return Results.Ok(response.Message.Text);
});*/

var buildersApi = app.MapGroup("/api/v1")
                     .WithTags("Builders")
                     .RequireAuthorization()
                     .RequireRateLimiting("builders-api")
                     .AddEndpointFilter<TenantAccessEndpointFilter>();

app.MapEndpoints(buildersApi);
PublishedApplicationAiEndpoints.Map(app.MapGroup("/api/v1").WithTags("Published application AI").RequireRateLimiting("builders-api"));
BuilderApplicationUserEndpoints.MapEndpoint(app.MapGroup("/api/v1").WithTags("Builders"));
BuilderWorkspaceEndpoints.Map(app.MapGroup("/api/v1").WithTags("Builders"));
TenantAccessEndpoints.Map(app.MapGroup("/api/v1").WithTags("Builders"));
SystemDashboardEndpoints.Map(app.MapGroup("/api/v1").WithTags("Builders"));

app.MapOpenApi().CacheOutput();

app.MapLogLevelHandler();

if (app.Environment.IsDevelopment())
    app.UseHangfireDashboard("/hangfire");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

if (File.Exists(Path.Combine(spaRoot, "index.html")))
{
    // Static assets are served by the earlier UseStaticFiles middleware.
    // Use a fallback endpoint for Angular routes instead of SpaDefaultPageMiddleware,
    // which can fail when the packaged SPA file provider differs from the web root.
    app.MapFallbackToFile("index.html");
}
else
{
    app.Logger.LogInformation(
        "Builder SPA assets were not found at {SpaRoot}; serving API routes without SPA fallback.",
        spaRoot);
}

app.Run();

