
using ANTSoftFederation.IdentityServer.Bootstraping;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseANTLogging();

builder.Services.AddCorrelationContextLogging();

builder.AddServiceDefaults();

// todo: Tps keyVault
// builder.Host.UseTpsKeyVault();

builder.AddApplicationServices();

builder.Services.AddEndPoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseCors("ANTFedPolicy");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


ApiVersionSet apiVersionSet = app.NewApiVersionSet()
                                 .HasApiVersion(new ApiVersion(1))
                                 .ReportApiVersions()
                                 .Build();

RouteGroupBuilder versionedGrp = app.MapGroup("api/v{version:apiVersion}")
                                    .WithApiVersionSet(apiVersionSet)
                                    ;

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

var figgleText = FiggleFonts.Standard.Render(builder.Configuration.GetSection("app:name").Value);

app.MapGet("/", ctx => ctx.Response.WriteAsync(figgleText));

app.MapOpenApi().CacheOutput();

app.MapScalarApiReference(o =>
{
    o.DarkMode = true;
    o.WithTitle("AntSoft Identity Server Docs");

});

app.MapLogLevelHandler();

// await app.MigrateDbContextAsync<ButterflyIdentityDbContext>();

var dataSeed = app.Services.GetService<IDbInitializerService>();
dataSeed.Initialize();

dataSeed.SeedGroupData();
dataSeed.SeedUserData();
dataSeed.SeedDefaultProject();
dataSeed.SeedDefaultPermission();

app.Run();