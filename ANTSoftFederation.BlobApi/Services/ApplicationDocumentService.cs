namespace SorteFederation.BlobApi.Services;

public sealed record StoredApplicationDocument(Guid Id, string Name, long Size, string Path);
public sealed record ApplicationDocumentUploadResult(Guid Id, string Name, long Size);

public sealed class ApplicationDocumentService(IWebHostEnvironment environment, IConfiguration configuration)
{
    private const long MaximumPdfSize = 5_000_000;

    public async Task<ApplicationDocumentUploadResult> SaveAsync(Guid tenantId, Guid applicationId, IFormFile file, CancellationToken token)
    {
        if (file.Length is <= 0 or > MaximumPdfSize)
            throw new ArgumentException("Upload a non-empty PDF no larger than 5 MB.");
        if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only PDF files are supported.");

        var id = Guid.CreateVersion7();
        var name = Path.GetFileName(file.FileName);
        var folder = Path.Combine(GetApplicationsRoot(), tenantId.ToString("N"), applicationId.ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, $"{id:N}.pdf");

        await using var input = file.OpenReadStream();
        var header = new byte[5];
        var headerLength = await input.ReadAsync(header.AsMemory(), token);
        if (headerLength != header.Length || !header.AsSpan().SequenceEqual("%PDF-"u8))
            throw new ArgumentException("The selected file is not a valid PDF.");
        input.Position = 0;

        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await input.CopyToAsync(output, token);
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }

        return new ApplicationDocumentUploadResult(id, name, file.Length);
    }

    public Task<StoredApplicationDocument?> FindAsync(Guid tenantId, Guid applicationId, Guid documentId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var path = Path.Combine(GetApplicationsRoot(), tenantId.ToString("N"), applicationId.ToString("N"), $"{documentId:N}.pdf");
        return Task.FromResult(File.Exists(path)
            ? new StoredApplicationDocument(documentId, $"{documentId:N}.pdf", new FileInfo(path).Length, path)
            : null);
    }

    private string GetApplicationsRoot()
    {
        var dataPath = configuration["DATA_PATH"];
        return string.IsNullOrWhiteSpace(dataPath)
            ? Path.Combine(environment.ContentRootPath, "Contents", "Applications")
            : Path.Combine(dataPath, "Applications");
    }
}
