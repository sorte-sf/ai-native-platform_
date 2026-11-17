using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;

namespace SorteFederation.Builders.Services;

public sealed class SolutionDocumentService(BuilderDbContext metadata, KnowledgeService knowledge,
    KnowledgeEmbeddingService embeddings, IHttpClientFactory clients, IHttpContextAccessor contextAccessor,
    IConfiguration configuration)
{
    public async Task<object> UploadAsync(Guid tenantId, Guid applicationId, IFormFile file, CancellationToken token)
    {
        if (!await metadata.Applications.AnyAsync(x => x.Id == applicationId && x.TenantId == tenantId, token))
            throw new KeyNotFoundException("Application was not found for this tenant.");
        if (file.Length is <= 0 or > 5_000_000)
            throw new ArgumentException("Upload a non-empty file no larger than 5 MB.");
        var name = Path.GetFileName(file.FileName);
        if (name.Length is < 1 or > 200) throw new ArgumentException("Invalid file name.");
        var extension = Path.GetExtension(name).ToLowerInvariant();
        if (extension is not (".txt" or ".md" or ".docx" or ".pdf"))
            throw new ArgumentException("Supported formats are TXT, MD, DOCX and PDF.");

        await using var input = file.OpenReadStream();
        var content = extension switch
        {
            ".docx" => ReadDocx(input),
            ".pdf" => ReadPdf(input),
            _ => await new StreamReader(input, Encoding.UTF8, true).ReadToEndAsync(token)
        };
        content = content.Trim();
        if (content.Length is < 1 or > 128_000)
            throw new ArgumentException("Extracted text must contain 1 to 128,000 characters.");
        StoredDocumentResponse? stored = null;
        if (extension == ".pdf")
        {
            var blob = clients.CreateClient("blob-documents");
            using var multipart = new MultipartFormDataContent();
            await using var uploadStream = file.OpenReadStream();
            var pdfContent = new StreamContent(uploadStream);
            pdfContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            multipart.Add(pdfContent, "file", name);
            var blobApiBaseUrl = (configuration["BlobApi:BaseUrl"] ?? "http://antsoft-blob-service").TrimEnd('/');
            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"{blobApiBaseUrl}/api/v1/applications/{applicationId}/documents?tenantId={tenantId}")
            { Content = multipart };
            var authorization = contextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authorization))
                request.Headers.TryAddWithoutValidation("Authorization", authorization);
            using var response = await blob.SendAsync(request, token);
            response.EnsureSuccessStatusCode();
            stored = await response.Content.ReadFromJsonAsync<StoredDocumentResponse>(cancellationToken: token)
                ?? throw new InvalidOperationException("Blob storage returned an invalid response.");
        }

        string? fileUrl = stored is null ? null : $"/blob/applications/{applicationId}/documents/{stored.Id}?tenantId={tenantId}";
        var document = await knowledge.AddAsync(new CreateKnowledgeDocumentRequest(
            tenantId, $"solution:{applicationId:N}:{Guid.NewGuid():N}", name, content, fileUrl ?? name,
            stored is null ? null : JsonSerializer.Serialize(new { stored.Id, stored.Name, stored.Size }), applicationId), token);
        await knowledge.PublishAsync(tenantId, document.Id, token);
        int indexed;
        try { indexed = await embeddings.IndexAsync(tenantId, document.Id, token); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new { document.Id, document.ApplicationId, document.Title, FileId = stored?.Id,
                FileUrl = fileUrl, ChunksIndexed = 0, IndexStatus = "pending" };
        }
        return new { document.Id, document.ApplicationId, document.Title, FileId = stored?.Id,
            FileUrl = fileUrl, ChunksIndexed = indexed, IndexStatus = indexed == 0 ? "pending" : "indexed" };
    }

    private static string ReadDocx(Stream input)
    {
        using var zip = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true);
        var entry = zip.GetEntry("word/document.xml") ?? throw new ArgumentException("Invalid DOCX file.");
        if (entry.Length > 2_000_000) throw new ArgumentException("DOCX text is too large.");
        using var xml = entry.Open();
        var root = XDocument.Load(xml);
        XNamespace ns = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        return string.Join("\n", root.Descendants(ns + "p")
            .Select(p => string.Concat(p.Descendants(ns + "t").Select(t => t.Value))));
    }

    private static string ReadPdf(Stream input)
    {
        using var pdf = PdfDocument.Open(input);
        if (pdf.NumberOfPages > 100) throw new ArgumentException("PDF exceeds 100 pages.");
        return string.Join("\n", pdf.GetPages().Select(page => page.Text));
    }

    private sealed record StoredDocumentResponse(Guid Id, string Name, long Size);
}
