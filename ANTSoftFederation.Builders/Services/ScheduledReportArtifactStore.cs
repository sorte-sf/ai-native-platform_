namespace SorteFederation.Builders.Services;

public sealed class ScheduledReportArtifactStore(IWebHostEnvironment environment, IConfiguration configuration)
{
    private const string LatestFileName = "latest.csv";
    private readonly string rootPath = ResolveRoot(environment, configuration);

    public async Task SaveLatestAsync(Guid jobId, ReadOnlyMemory<byte> content, CancellationToken cancellationToken)
    {
        if (jobId == Guid.Empty) throw new ArgumentException("Job ID is required.", nameof(jobId));
        var directory = Path.Combine(rootPath, jobId.ToString("N"));
        Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, LatestFileName);
        var temporary = Path.Combine(directory, $"{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                await stream.WriteAsync(content, cancellationToken);
            File.Move(temporary, target, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public string? GetLatestPath(Guid jobId)
    {
        if (jobId == Guid.Empty) return null;
        var path = Path.GetFullPath(Path.Combine(rootPath, jobId.ToString("N"), LatestFileName));
        if (!path.StartsWith(rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return null;
        return File.Exists(path) ? path : null;
    }

    private static string ResolveRoot(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var configuredPath = configuration["ScheduledJobs:Reports:StoragePath"];
        var path = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "ScheduledReports")
            : Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(environment.ContentRootPath, configuredPath);
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
