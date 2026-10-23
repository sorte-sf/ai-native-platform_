
namespace SorteFederation.Builders.Data;

public sealed class TenantRuntimeDbContextFactory : IDesignTimeDbContextFactory<TenantRuntimeDbContext>
{
    public TenantRuntimeDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__SorteBuilderDb2026")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__builders-db")
            ?? "Host=localhost;Port=5432;Database=sorte_tenant_runtime;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<TenantRuntimeDbContext>()
            .UseNpgsql(connection, npgsql => npgsql.UseVector()
                .MigrationsAssembly(typeof(TenantRuntimeDbContext).Assembly.FullName)
                .MigrationsHistoryTable("__EFMigrationsHistory", "builders_runtime"))
            .Options;
        return new TenantRuntimeDbContext(options);
    }
}
