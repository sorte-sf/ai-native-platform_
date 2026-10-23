
using SorteFederation.Shared;
using System.Reflection; 

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); 

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
}); 

builder.Services.AddCors(opt => opt.AddPolicy("SortePolicy", builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

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
//app.UseHttpsRedirection();
 
app.Run();
