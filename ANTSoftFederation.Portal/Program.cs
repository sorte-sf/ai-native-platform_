
using SorteFederation.Portal;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.UseANTLogging();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();

builder.Services.AddHttpClient("portal", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

builder.Services.AddSingleton<ApiPerformanceMetrics>();

// todo: Tps keyVault
// builder.Host.UseTpsKeyVault("Antsoft", "vault");

// Stdio owns the process lifetime and is intended for a console MCP host.
// Aspire runs this project as an HTTP API without an interactive stdin stream.
if (builder.Configuration.GetValue<bool>("Mcp:EnableStdioTransport"))
    builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly();

// builder.AddOllamaApiClient("ollama-qwen3-embedding").AddEmbeddingGenerator();
 
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

var app = builder.Build();

/*app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; object-src 'none'; frame-src 'self'; base-uri 'self'";
    await next();
});*/

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
                                 .HasApiVersion(new ApiVersion(1))
                                 .ReportApiVersions()
                                 .Build();

RouteGroupBuilder versionedGrp = app.MapGroup("api/v{version:apiVersion}")
                                    .WithApiVersionSet(apiVersionSet);

versionedGrp.MapAuthEndpoints();
versionedGrp.MapAddonManifestEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapEndpointsWithoutAuth(versionedGrp);
}
else
{
    app.UseHsts();
    app.MapEndpoints(versionedGrp);
}

app.MapScalarApiReference(o =>
{
    o.DarkMode = true;
    o.WithTitle("Sorte API Docs");
    o.WithTitle("Sorte API Documentation")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

    o.Authentication = new ScalarAuthenticationOptions
    {

    };
});


app.UseStaticFiles();
app.UseCors("SortePolicy");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapOpenApi().CacheOutput();
app.UseHttpsRedirection();

app.MapLogLevelHandler();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

/*var dataSeed = app.Services.GetService<IDbInitializerService>();
dataSeed.Initialize();
dataSeed.SeedApplications();*/

await using (var scope = app.Services.CreateAsyncScope())
{
    var masterDb = scope.ServiceProvider.GetRequiredService<ANTSoftPortalDbContext>();
    await masterDb.Database.MigrateAsync();

    await MasterSeed.SeedAsync(
        masterDb,
        scope.ServiceProvider.GetRequiredService<ISecurityService>(),
        scope.ServiceProvider.GetRequiredService<SorteFederation.Shared.Realtime.IRealtimeEventBus>(),
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>(),
        CancellationToken.None);
}

app.Map("/mcp", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();

        //await McpWebSocketProcessor.ProcessAsync(socket, context.RequestAborted);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
}); 

app.UseSpa(spa =>
{
    spa.ApplicationBuilder.UseStaticFiles();
    spa.Options.SourcePath = "wwwroot"; 
});

app.Run();
