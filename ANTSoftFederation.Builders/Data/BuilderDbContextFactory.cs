using SorteFederation.EncryptEFCoreColumn;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SorteFederation.Builders.Data;

public sealed class BuilderDbContextFactory : IDesignTimeDbContextFactory<BuilderDbContext>
{
    private readonly IEncryptionProvider _provider;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public BuilderDbContextFactory()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var encryptionKey = configuration["Encryption:Key"];
        if (string.IsNullOrWhiteSpace(encryptionKey) || System.Text.Encoding.UTF8.GetByteCount(encryptionKey) is not (16 or 24 or 32))
            throw new InvalidOperationException("Encryption:Key must be configured as a 16, 24, or 32-byte AES key for EF design-time operations.");
        _provider = new GenerateEncryptionProvider(encryptionKey);
        _httpContextAccessor = new HttpContextAccessor();
    }

    public BuilderDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__SortePlatformBuilderDb2027")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__builders-db")
            ?? "Host=localhost;Port=5432;Database=sorte_builders;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<BuilderDbContext>().UseNpgsql(connection).Options;
        return new BuilderDbContext(options, _provider, _httpContextAccessor);
    }
}
