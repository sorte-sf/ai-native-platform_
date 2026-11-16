
global using SorteFederation.Logger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
 
builder.Host.UseANTLogging();

builder.AddApplicationServices();

var app = builder.Build();
 
var imageDirectory = Path.Combine(builder.Environment.ContentRootPath, "Images");
if (!Directory.Exists(imageDirectory))
    Directory.CreateDirectory(imageDirectory);

var videosDirectory = Path.Combine(builder.Environment.ContentRootPath, "Videos");
if (!Directory.Exists(videosDirectory))
    Directory.CreateDirectory(videosDirectory);

var docsDirectory = Path.Combine(builder.Environment.ContentRootPath, "Documents");
if (!Directory.Exists(docsDirectory))
    Directory.CreateDirectory(docsDirectory);

var applicationDocsDirectory = Path.Combine(builder.Environment.ContentRootPath, "AppDocs");
if (!Directory.Exists(applicationDocsDirectory))
    Directory.CreateDirectory(applicationDocsDirectory);

var imageDirectoryProvider = new PhysicalFileProvider(imageDirectory);
var videoDirectoryProvider = new PhysicalFileProvider(videosDirectory);

var photoGalleryDirectoryProvider = new PhysicalFileProvider(videosDirectory);

if (!Directory.Exists(photoGalleryDirectoryProvider.Root))
    Directory.CreateDirectory(photoGalleryDirectoryProvider.Root);

var figgleText = FiggleFonts.Doom.Render(builder.Configuration.GetSection("app:name").Value); 
app.MapGet("/", ctx => ctx.Response.WriteAsync(figgleText)); 

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
                                 .HasApiVersion(new ApiVersion(1))
                                 .ReportApiVersions()
                                 .Build();

RouteGroupBuilder versionedGrp = app.MapGroup("api/v{version:apiVersion}")
                                    .WithApiVersionSet(apiVersionSet)
                                    ;

app.MapBlobApi();

app.MapOpenApi().CacheOutput();

app.MapScalarApiReference(o =>
{
    o.DarkMode = true;

    //o.WithTitle($"{builder.Configuration.GetSection("app:name").Value} Documentations").WithSidebar(true);

    o.Authentication = new ScalarAuthenticationOptions
    {

    };

});

app.UseRouting();

app.UseCors("SortePolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

//app.UseHttpsRedirection();

app.MapLogLevelHandler();

await using (var scope = app.Services.CreateAsyncScope())
{
    var blobDb = scope.ServiceProvider.GetRequiredService<BlobDbContext>();
    await blobDb.Database.MigrateAsync();
}

app.Run();
