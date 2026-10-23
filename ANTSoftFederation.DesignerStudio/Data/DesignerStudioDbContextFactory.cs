using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SorteFederation.DesignerStudio.Data;

public sealed class DesignerStudioDbContextFactory : IDesignTimeDbContextFactory<DesignerStudioDbContext>
{
    public DesignerStudioDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DesignerStudioDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("DESIGNER_STUDIO_MIGRATIONS_CONNECTION")
            ?? "Filename=./Contents/Data/sorteDesignStudio.db";
        options.UseSqlite(connectionString);
        return new DesignerStudioDbContext(options.Options);
    }
}
