using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pgvector.EntityFrameworkCore;

namespace SorteFederation.BlobApi.DataDbContext;

/// <summary>
/// Creates the Blob API context for EF tooling without starting the web host,
/// so migrations do not depend on runtime-only configuration such as JWT keys.
/// </summary>
public sealed class BlobDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BlobDbContext>
{
    public BlobDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(ResolveConfigurationPath())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection must be configured to create BlobDbContext for migrations.");

        var options = new DbContextOptionsBuilder<BlobDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.UseVector())
            .Options;

        return new BlobDbContext(options, new HttpContextAccessor());
    }

    private static string ResolveConfigurationPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        if (File.Exists(Path.Combine(currentDirectory, "appsettings.json")))
            return currentDirectory;

        var outputDirectory = AppContext.BaseDirectory;
        if (File.Exists(Path.Combine(outputDirectory, "appsettings.json")))
            return outputDirectory;

        throw new InvalidOperationException(
            "Could not locate Blob API appsettings.json. Run EF migrations from the BlobApi project directory or build the project first.");
    }
}
