using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace SorteFederation.DesignerStudio.Storage;

public sealed class FileSystemTemplatePackageStorage : ITemplatePackageStorage
{
    public const long MaximumPackageBytes = 50L * 1024 * 1024;
    private const long MaximumExpandedBytes = 250L * 1024 * 1024;
    private const int MaximumEntries = 2_000;
    private const int MaximumManifestBytes = 1024 * 1024;
    private readonly string _root;

    public FileSystemTemplatePackageStorage(string root)
    {
        _root = Path.GetFullPath(root);
        Directory.CreateDirectory(_root);
    }

    public async Task<StoredTemplatePackage> StoreAsync(
        IFormFile file,
        Guid releaseId,
        string templateKey,
        string templateName,
        string version,
        CancellationToken cancellationToken)
    {
        if (file.Length <= 0 || file.Length > MaximumPackageBytes)
            throw new ArgumentException($"Package file must be between 1 byte and {MaximumPackageBytes / 1024 / 1024} MB.");
        if (!Path.GetExtension(file.FileName).Equals(".anttemplate", StringComparison.OrdinalIgnoreCase)
            && !Path.GetExtension(file.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Upload a .anttemplate package (ZIP format) or a .zip file.");

        var finalName = $"{releaseId:N}-{Guid.NewGuid():N}.anttemplate";
        var finalPath = Path.Combine(_root, finalName);
        var temporaryPath = Path.Combine(_root, $".{releaseId:N}.{Guid.NewGuid():N}.upload");

        try
        {
            await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            await using (var input = file.OpenReadStream())
            {
                await input.CopyToAsync(output, cancellationToken);
            }

            var length = new FileInfo(temporaryPath).Length;
            if (length <= 0 || length > MaximumPackageBytes)
                throw new ArgumentException("The uploaded package exceeds the allowed size.");

            var manifest = ValidateArchive(temporaryPath, templateKey, templateName, version);
            string sha256;
            await using (var stream = File.OpenRead(temporaryPath))
            {
                sha256 = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken)).ToLowerInvariant();
            }

            File.Move(temporaryPath, finalPath, overwrite: false);
            return new StoredTemplatePackage(finalName, sha256, length, manifest);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    public Stream OpenRead(string packagePath)
    {
        var safePath = ResolveStoredPath(packagePath);
        return new FileStream(safePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
    }

    public void Delete(string packagePath)
    {
        var safePath = ResolveStoredPath(packagePath);
        if (File.Exists(safePath)) File.Delete(safePath);
    }

    private string ResolveStoredPath(string packagePath)
    {
        if (string.IsNullOrWhiteSpace(packagePath) || Path.GetFileName(packagePath) != packagePath)
            throw new InvalidOperationException("The stored package path is invalid.");
        return Path.Combine(_root, packagePath);
    }

    private static string ValidateArchive(string archivePath, string templateKey, string templateName, string version)
    {
        try
        {
            using var archive = ZipFile.OpenRead(archivePath);
            if (archive.Entries.Count == 0 || archive.Entries.Count > MaximumEntries)
                throw new ArgumentException($"A package must contain between 1 and {MaximumEntries} files.");

            long expandedBytes = 0;
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ZipArchiveEntry? manifestEntry = null;
            foreach (var entry in archive.Entries)
            {
                var normalized = entry.FullName.Replace('\\', '/');
                if (normalized.StartsWith('/') || normalized.Contains('\0') || Path.IsPathRooted(normalized)
                    || normalized.Split('/').Any(segment => segment is ".." or "."))
                    throw new ArgumentException("The package contains an unsafe file path.");
                if (!seenPaths.Add(normalized))
                    throw new ArgumentException("The package contains duplicate file paths.");

                expandedBytes = checked(expandedBytes + entry.Length);
                if (expandedBytes > MaximumExpandedBytes)
                    throw new ArgumentException("The expanded package content exceeds 250 MB.");

                if (normalized.Equals("manifest.json", StringComparison.OrdinalIgnoreCase))
                    manifestEntry = entry;
            }

            if (manifestEntry is null || manifestEntry.Length is <= 0 or > MaximumManifestBytes)
                throw new ArgumentException("The package must contain a root-level manifest.json smaller than 1 MB.");

            using var manifestStream = manifestEntry.Open();
            using var document = JsonDocument.Parse(manifestStream, new JsonDocumentOptions { MaxDepth = 48 });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("formatVersion", out var formatVersion)
                || !formatVersion.TryGetInt32(out var format) || format != 1)
                throw new ArgumentException("manifest.json must declare formatVersion 1.");

            if (!root.TryGetProperty("template", out var template) || template.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("manifest.json must contain a template object.");
            EnsureMatches(template, "key", templateKey, "template key");
            EnsureMatches(template, "name", templateName, "template name");
            EnsureMatches(template, "version", version, "template version");

            if (!root.TryGetProperty("dataModels", out var models) || models.ValueKind != JsonValueKind.Array || models.GetArrayLength() == 0)
                throw new ArgumentException("Define at least one package-owned data model in manifest.json > dataModels.");

            var fieldsByModel = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var model in models.EnumerateArray())
            {
                var modelKey = RequiredString(model, "key", "Every data model needs a key.");
                _ = RequiredString(model, "name", $"Data model '{modelKey}' needs a name.");
                if (!model.TryGetProperty("fields", out var fields) || fields.ValueKind != JsonValueKind.Array || fields.GetArrayLength() == 0)
                    throw new ArgumentException($"Data model '{modelKey}' must define at least one field.");

                var fieldKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var field in fields.EnumerateArray())
                {
                    var fieldKey = RequiredString(field, "key", $"Every field in '{modelKey}' needs a key.");
                    _ = RequiredString(field, "type", $"Field '{modelKey}.{fieldKey}' needs a type.");
                    if (!fieldKeys.Add(fieldKey))
                        throw new ArgumentException($"Data model '{modelKey}' has duplicate field key '{fieldKey}'.");
                }
                if (!fieldsByModel.TryAdd(modelKey, fieldKeys))
                    throw new ArgumentException($"Duplicate data model key '{modelKey}'.");
            }

            if (root.TryGetProperty("bindings", out var bindings))
            {
                if (bindings.ValueKind != JsonValueKind.Array)
                    throw new ArgumentException("manifest.json > bindings must be an array.");
                foreach (var binding in bindings.EnumerateArray())
                {
                    var modelKey = RequiredString(binding, "modelKey", "Every binding needs a modelKey.");
                    var fieldKey = RequiredString(binding, "fieldKey", "Every binding needs a fieldKey.");
                    if (!fieldsByModel.TryGetValue(modelKey, out var fields) || !fields.Contains(fieldKey))
                        throw new ArgumentException($"Binding '{modelKey}.{fieldKey}' does not resolve to a field defined in this package.");
                }
            }

            return JsonSerializer.Serialize(root);
        }
        catch (InvalidDataException exception)
        {
            throw new ArgumentException("The uploaded file is not a valid ZIP-based template package.", exception);
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("The package manifest is not valid JSON.", exception);
        }
    }

    private static void EnsureMatches(JsonElement element, string property, string expected, string label)
    {
        var value = RequiredString(element, property, $"The manifest {label} is required.");
        if (!value.Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"The manifest {label} must match the submitted {label}.");
    }

    private static string RequiredString(JsonElement element, string property, string error)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(property, out var value)
            || value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
            throw new ArgumentException(error);
        return value.GetString()!.Trim();
    }
}
