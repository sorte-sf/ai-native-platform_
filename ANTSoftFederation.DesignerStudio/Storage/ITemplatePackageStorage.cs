namespace SorteFederation.DesignerStudio.Storage;

public interface ITemplatePackageStorage
{
    Task<StoredTemplatePackage> StoreAsync(IFormFile file, Guid releaseId, string templateKey, string templateName, string version, CancellationToken cancellationToken);
    Stream OpenRead(string packagePath);
    void Delete(string packagePath);
}

public sealed record StoredTemplatePackage(string Path, string Sha256, long SizeBytes, string ManifestJson);
