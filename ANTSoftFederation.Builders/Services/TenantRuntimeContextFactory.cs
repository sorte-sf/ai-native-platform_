using System.Security.Claims;
using SorteFederation.Builders.Data;

namespace SorteFederation.Builders.Services;

/// <summary>Creates tenant-scoped contexts against one shared Builders database.</summary>
public sealed class TenantRuntimeContextFactory(IConfiguration configuration, IHttpContextAccessor accessor)
{
    public Task<TenantRuntimeDbContext> CreateAsync(Guid tenantId, CancellationToken token)
    {
        var user = accessor.HttpContext?.User;
        var claim = user?.FindFirstValue("tenant_id") ?? user?.FindFirstValue("company_id") ?? user?.FindFirstValue("companyId");
        if (user?.Identity?.IsAuthenticated != true ||
            (!user.IsInRole(BuilderRoleNames.SystemAdmin) &&
             (!Guid.TryParse(claim, out var tokenTenant) || tokenTenant != tenantId)))
            throw new UnauthorizedAccessException("The requested tenant does not match the authenticated tenant.");

        return CreateForWorkerAsync(tenantId, token);
    }

    public Task<TenantRuntimeDbContext> CreateForWorkerAsync(Guid tenantId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (tenantId == Guid.Empty) throw new ArgumentException("A tenant id is required.", nameof(tenantId));
        var connection = configuration.GetConnectionString("SortePlatformBuilderDb2027")
            ?? configuration.GetConnectionString("builders-db")
            ?? throw new InvalidOperationException("The shared Builders PostgreSQL connection is required.");
        var options = new DbContextOptionsBuilder<TenantRuntimeDbContext>()
            .UseNpgsql(connection, npgsql => npgsql.UseVector()
                .MigrationsAssembly(typeof(TenantRuntimeDbContext).Assembly.FullName)
                .MigrationsHistoryTable("__EFMigrationsHistory", "builders_runtime")
                .UseQuerySplittingBehavior(Microsoft.EntityFrameworkCore.QuerySplittingBehavior.SplitQuery))
            .Options;
        return Task.FromResult(new TenantRuntimeDbContext(options, tenantId));
    }
}
