using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Services;

public static class TemplateReleaseService
{
    public static async Task<BuilderTemplateRelease?> GetLatestReleaseAsync(
        IQueryable<BuilderTemplateRelease> releases, CancellationToken token = default)
    {
        var items = await releases.ToListAsync(token);
        return items.OrderByDescending(x => x, ReleaseComparer).FirstOrDefault();
    }

    public static IComparer<BuilderTemplateRelease> VersionComparer => ReleaseComparer;

    public static async Task<BuilderTemplateRelease> EnsureReleaseAsync(
        BuilderDbContext db, BuilderTemplate template, CancellationToken token = default)
    {
        var version = string.IsNullOrWhiteSpace(template.BlueprintVersion) ? "1.0.0" : template.BlueprintVersion;
        var existing = await db.TemplateReleases.SingleOrDefaultAsync(
            x => x.TemplateId == template.Id && x.ReleaseVersion == version, token);
        if (existing is not null)
        {
            if (existing.ManifestVersion != template.ManifestVersion ||
                existing.ManifestJson != template.ManifestJson || existing.ThemeJson != template.ThemeJson ||
                existing.HtmlContent != template.HtmlContent || existing.ScssContent != template.ScssContent ||
                existing.Description != template.Description || existing.PreviewImageUrl != template.PreviewImageUrl ||
                existing.BaseTemplate != template.BaseTemplate)
                throw new InvalidOperationException($"Template '{template.Name}' changed without a release-version bump. Create version {NextPatch(version)} before publishing these changes.");
            return existing;
        }

        var release = new BuilderTemplateRelease
        {
            TemplateId = template.Id,
            ReleaseVersion = version,
            ManifestVersion = template.ManifestVersion,
            ManifestJson = template.ManifestJson,
            ThemeJson = template.ThemeJson,
            HtmlContent = template.HtmlContent,
            ScssContent = template.ScssContent,
            Description = template.Description,
            PreviewImageUrl = template.PreviewImageUrl,
            BaseTemplate = template.BaseTemplate
        };
        db.TemplateReleases.Add(release);
        return release;
    }

    public static string NextPatch(string version)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(version, @"\A(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)\z"))
            return "1.0.0";
        var parts = version.Split('.');
        return $"{parts[0]}.{parts[1]}.{(System.Numerics.BigInteger.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture) + 1)}";
    }

    private sealed class ReleaseVersionComparer : IComparer<BuilderTemplateRelease>
    {
        public int Compare(BuilderTemplateRelease? left, BuilderTemplateRelease? right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left is null) return -1;
            if (right is null) return 1;

            var leftValid = TryParse(left.ReleaseVersion, out var leftParts);
            var rightValid = TryParse(right.ReleaseVersion, out var rightParts);
            if (leftValid && rightValid)
            {
                for (var i = 0; i < 3; i++)
                {
                    var partComparison = leftParts[i].CompareTo(rightParts[i]);
                    if (partComparison != 0) return partComparison;
                }
            }
            else if (leftValid != rightValid)
            {
                return leftValid ? 1 : -1;
            }

            var dateComparison = left.ReleasedAt.CompareTo(right.ReleasedAt);
            return dateComparison != 0 ? dateComparison : left.Id.CompareTo(right.Id);
        }

        private static bool TryParse(string version, out System.Numerics.BigInteger[] parts)
        {
            parts = [];
            if (!System.Text.RegularExpressions.Regex.IsMatch(version, @"\A(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)\z")) return false;
            parts = version.Split('.').Select(x => System.Numerics.BigInteger.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            return true;
        }
    }

    private static readonly IComparer<BuilderTemplateRelease> ReleaseComparer = new ReleaseVersionComparer();
}
