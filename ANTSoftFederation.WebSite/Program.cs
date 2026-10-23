
using SorteFederation.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

builder.Services.ConfigureCorsPolicy();

// Build the application
var app = builder.Build();

app.UseDefaultFiles(
      new DefaultFilesOptions { DefaultFileNames = new List<string> { "index.html" } }
 );

app.UseStaticFiles();
app.UseCors("SortePolicy");
app.UseRouting();

app.UseSpa(spa =>
{
    spa.ApplicationBuilder.UseStaticFiles();
    spa.Options.SourcePath = "wwwroot"; // Change if needed
});

// Use HTTPS redirection
app.UseHttpsRedirection();

// Run the application
app.Run();
