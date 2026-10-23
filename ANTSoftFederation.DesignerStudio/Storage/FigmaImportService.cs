using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SorteFederation.DesignerStudio.Storage;

public sealed class FigmaImportService(HttpClient http, IConfiguration configuration)
{
    private const int MaximumResponseBytes = 8 * 1024 * 1024;
    private const int MaximumVisitedNodes = 10_000;
    private const int MaximumTreeDepth = 64;
    private static readonly Regex FileKeyPattern = new("^[A-Za-z0-9_-]{10,128}$", RegexOptions.Compiled);
    private static readonly Regex NodeIdPattern = new("^\\d+:\\d+$", RegexOptions.Compiled);

    private readonly string figmaUrl = configuration["services:Figma-Api:default:0"] ?? "https://api.figma.com/v1/files";

    public async Task<FigmaImportResult> ImportAsync(string fileUrl, string accessToken, string? nodeId, CancellationToken cancellationToken)
    {
        var fileKey = ParseFileKey(fileUrl);
        if (string.IsNullOrWhiteSpace(accessToken) || accessToken.Length > 1024 || accessToken.Any(char.IsControl))
            throw new ArgumentException("Enter a valid Figma access token with file_content:read permission.");

        if (nodeId is not null && !NodeIdPattern.IsMatch(nodeId))
            throw new ArgumentException("Choose a valid Figma frame before importing.");

        var fileEndpoint = $"{figmaUrl.TrimEnd('/')}/{Uri.EscapeDataString(fileKey)}";
        var requestUrl = nodeId is null
            ? $"{fileEndpoint}?depth=2"
            : $"{fileEndpoint}/nodes?ids={Uri.EscapeDataString(nodeId)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.Add("X-Figma-Token", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FigmaImportException("The request to Figma timed out. Check server connectivity and retry.", 504);
        }
        catch (HttpRequestException)
        {
            throw new FigmaImportException("Designer Studio could not reach Figma. Check that the API server has outbound internet access and retry.", 502);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var status = (int)response.StatusCode;
                var message = status switch
                {
                    401 => "Figma rejected the access token. Create or replace it with a valid token, then retry.",
                    403 => "Figma denied access. Confirm the token has file_content:read scope and its account can open this file.",
                    404 => "Figma could not find this file. Verify the file key and confirm the token's account has access.",
                    429 => "Figma rate-limited this import. Please wait a moment and try again.",
                    _ => $"Figma returned HTTP {status}. Please retry; if it continues, check Figma availability and API access."
                };
                throw new FigmaImportException(message, status is 401 or 403 or 404 ? 400 : 502);
            }

        if (response.Content.Headers.ContentLength is > MaximumResponseBytes)
            throw new FigmaImportException("This Figma file is too large to import in one operation (8 MB limit).", 413);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[32 * 1024];
        while (true)
        {
            var count = await stream.ReadAsync(chunk, cancellationToken);
            if (count == 0) break;
            if (buffer.Length + count > MaximumResponseBytes)
                throw new FigmaImportException("This Figma file is too large to import in one operation (8 MB limit).", 413);
            await buffer.WriteAsync(chunk.AsMemory(0, count), cancellationToken);
        }

        using var document = JsonDocument.Parse(buffer.ToArray());
        var frames = new List<FigmaImportFrame>();
        if (nodeId is null)
        {
            if (!document.RootElement.TryGetProperty("document", out var root) || root.ValueKind != JsonValueKind.Object)
                throw new FigmaImportException("Figma returned a file without a readable design document.", 422);
            if (root.TryGetProperty("children", out var pages) && pages.ValueKind == JsonValueKind.Array)
            {
                foreach (var page in pages.EnumerateArray())
                {
                    if (frames.Count >= 100) break;
                    if (!page.TryGetProperty("children", out var nodes) || nodes.ValueKind != JsonValueKind.Array) continue;
                    foreach (var node in nodes.EnumerateArray())
                    {
                        if (GetString(node, "type") is not ("FRAME" or "COMPONENT" or "SECTION")) continue;
                        var frameId = GetString(node, "id");
                        if (frameId is null) continue;
                        frames.Add(CreateFrame(node, []));
                    }
                    if (frames.Count >= 100) break;
                }
            }
        }
        else
        {
            if (!document.RootElement.TryGetProperty("nodes", out var nodes) || nodes.ValueKind != JsonValueKind.Object ||
                !nodes.TryGetProperty(nodeId, out var selected) || selected.ValueKind != JsonValueKind.Object ||
                !selected.TryGetProperty("document", out var frameNode) || frameNode.ValueKind != JsonValueKind.Object)
                throw new FigmaImportException("Figma did not return the selected frame. Refresh the frame list and try again.", 422);

            var elements = new List<FigmaImportElement>();
            var frameBounds = GetBounds(frameNode, 0, 0);
            var visitedNodes = 0;
            Flatten(frameNode, "", elements, 0, frameBounds.X, frameBounds.Y, frameBounds.X, frameBounds.Y, ref visitedNodes);
            frames.Add(CreateFrame(frameNode, elements));
        }
        if (frames.Count == 0) throw new FigmaImportException("No top-level frames were found. Add a frame to the Figma page and retry.", 422);
        return new FigmaImportResult(GetString(document.RootElement, "name") ?? "Figma design", frames);
        }
    }

    private static void Flatten(JsonElement node, string parent, List<FigmaImportElement> elements, int depth,
        double originX, double originY, double parentX, double parentY, ref int visitedNodes)
    {
        if (depth > MaximumTreeDepth)
            throw new FigmaImportException($"The selected Figma frame is nested more than {MaximumTreeDepth} levels and cannot be imported safely.", 422);
        if (++visitedNodes > MaximumVisitedNodes)
            throw new FigmaImportException($"The selected Figma frame contains more than {MaximumVisitedNodes:N0} layers. Split it into smaller frames and import them separately.", 422);
        if (node.TryGetProperty("visible", out var visible) && visible.ValueKind == JsonValueKind.False) return;
        var type = GetString(node, "type") ?? "";
        var name = GetString(node, "name") ?? "Element";
        var path = parent.Length == 0 ? name : $"{parent} / {name}";
        var bounds = GetBounds(node, parentX, parentY);
        var kind = type switch
        {
            "TEXT" => GetSize(node, "fontSize") >= 24 ? "Heading" : "Text",
            "FRAME" or "COMPONENT" or "INSTANCE" or "GROUP" or "SECTION" or "RECTANGLE" => "Group",
            "ELLIPSE" or "VECTOR" or "STAR" or "POLYGON" or "LINE" => "Icon",
            _ => null
        };
        if (kind is not null)
        {
            var text = GetString(node, "characters") ?? (kind == "Icon" ? "✦" : name);
            elements.Add(new FigmaImportElement(
                GetString(node, "id") ?? Guid.NewGuid().ToString("N"), kind, name, path, text,
                ReadFill(node), ReadTextColor(node), GetSize(node, "fontSize"), GetSize(node, "fontWeight"), ReadCornerRadius(node),
                Math.Max(0, bounds.X - originX), Math.Max(0, bounds.Y - originY), bounds.Width, bounds.Height));
        }
        if (node.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array)
            foreach (var child in children.EnumerateArray())
                Flatten(child, path, elements, depth + 1, originX, originY, bounds.X, bounds.Y, ref visitedNodes);
    }

    private static FigmaImportFrame CreateFrame(JsonElement node, IReadOnlyList<FigmaImportElement> elements) =>
        new(GetString(node, "id") ?? throw new FigmaImportException("Figma returned a frame without an ID.", 422),
            GetString(node, "name") ?? "Figma frame", GetSize(node, "width"), GetSize(node, "height"), elements);

    private static (double X, double Y, double Width, double Height) GetBounds(JsonElement node, double parentX, double parentY)
    {
        if (node.TryGetProperty("absoluteBoundingBox", out var bounds) && bounds.ValueKind == JsonValueKind.Object)
            return (ReadCoordinate(bounds, "x"), ReadCoordinate(bounds, "y"), ReadNumber(bounds, "width"), ReadNumber(bounds, "height"));

        var (relativeX, relativeY) = ReadRelativePosition(node);
        var width = ReadSizeValue(node, "width");
        var height = ReadSizeValue(node, "height");
        return (parentX + relativeX, parentY + relativeY, width, height);
    }

    private static (double X, double Y) ReadRelativePosition(JsonElement node)
    {
        if (!node.TryGetProperty("relativeTransform", out var transform) || transform.ValueKind != JsonValueKind.Array || transform.GetArrayLength() < 2)
            return (0, 0);
        var firstRow = transform[0];
        var secondRow = transform[1];
        if (firstRow.ValueKind != JsonValueKind.Array || firstRow.GetArrayLength() < 3 ||
            secondRow.ValueKind != JsonValueKind.Array || secondRow.GetArrayLength() < 3)
            return (0, 0);
        return (ReadCoordinate(firstRow[2]), ReadCoordinate(secondRow[2]));
    }

    private static double ReadSizeValue(JsonElement node, string property)
    {
        if (node.TryGetProperty("size", out var size) && size.ValueKind == JsonValueKind.Object)
        {
            var sizeProperty = property == "width" ? "x" : "y";
            var value = ReadNumber(size, sizeProperty);
            if (value > 0) return value;
        }
        return node.TryGetProperty("style", out var style) && style.ValueKind == JsonValueKind.Object
            ? ReadNumber(style, property)
            : 0;
    }

    private static double ReadNumber(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.TryGetDouble(out var number)
            ? Math.Clamp(number, 0, 10000)
            : 0;

    private static double ReadCoordinate(JsonElement value) =>
        value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number)
            ? Math.Clamp(number, -10000, 10000)
            : 0;

    private static double ReadCoordinate(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) ? ReadCoordinate(value) : 0;

    private static string? ReadTextColor(JsonElement node) => ReadSolidColor(node, "fills");

    private static string? ReadFill(JsonElement node)
    {
        var fillsColor = ReadSolidColor(node, "fills");
        return GetString(node, "type") == "TEXT" ? null : fillsColor;
    }

    private static string? ReadSolidColor(JsonElement node, string property)
    {
        if (!node.TryGetProperty(property, out var fills) || fills.ValueKind != JsonValueKind.Array) return null;
        foreach (var fill in fills.EnumerateArray())
        {
            if (GetString(fill, "type") != "SOLID" || !fill.TryGetProperty("color", out var color) || color.ValueKind != JsonValueKind.Object) continue;
            var r = Channel(color, "r"); var g = Channel(color, "g"); var b = Channel(color, "b");
            if (r is null || g is null || b is null) continue;
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        return null;
    }

    private static int? Channel(JsonElement color, string key) => color.TryGetProperty(key, out var value) && value.TryGetDouble(out var component)
        ? (int)Math.Round(Math.Clamp(component, 0, 1) * 255) : null;

    private static double GetSize(JsonElement node, string property) =>
        node.TryGetProperty("absoluteBoundingBox", out var bounds) && bounds.TryGetProperty(property, out var size) && size.TryGetDouble(out var number)
            ? Math.Clamp(number, 0, 10000)
            : node.TryGetProperty("style", out var style) && style.TryGetProperty(property, out size) && size.TryGetDouble(out number)
                ? Math.Clamp(number, 0, 1000) : 0;

    private static double? ReadCornerRadius(JsonElement node) => node.TryGetProperty("cornerRadius", out var value) && value.TryGetDouble(out var radius)
        ? Math.Clamp(radius, 0, 256) : null;

    private static string? GetString(JsonElement node, string property) => node.ValueKind == JsonValueKind.Object &&
        node.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static string ParseFileKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Paste a Figma design URL or file key.");
        var candidate = value.Trim();
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme != Uri.UriSchemeHttps || uri.Host is not ("figma.com" or "www.figma.com") || !string.IsNullOrEmpty(uri.UserInfo))
                throw new ArgumentException("Use an HTTPS URL from figma.com.");
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            candidate = segments.Length >= 2 && segments[0] is "design" or "file" or "proto" ? segments[1] : "";
        }
        if (!FileKeyPattern.IsMatch(candidate)) throw new ArgumentException("That does not look like a Figma design URL or file key.");
        return candidate;
    }
}

public sealed record FigmaImportResult(string FileName, IReadOnlyList<FigmaImportFrame> Frames);
public sealed record FigmaImportFrame(string NodeId, string Name, double Width, double Height, IReadOnlyList<FigmaImportElement> Elements);
public sealed record FigmaImportElement(string Id, string Kind, string Name, string Path, string Text,
    string? BackgroundColor, string? Color, double FontSize, double FontWeight, double? BorderRadius,
    double X, double Y, double Width, double Height);
public sealed class FigmaImportException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
