using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SorteFederation.Builders.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Pgvector;

namespace SorteFederation.Builders.Services;

/// <summary>Indexes knowledge chunks using Aspire's embedding generator or a configured OpenAI-compatible HTTPS endpoint.</summary>
public sealed class KnowledgeEmbeddingService(
    TenantRuntimeContextFactory factory, IHttpClientFactory clients, IConfiguration configuration, ISecretResolver secrets,
    IEmbeddingGenerator<string, Embedding<float>> generator)
{
    public async Task<int> IndexAsync(Guid tenantId, Guid documentId, CancellationToken token)
    {
        var model = string.IsNullOrWhiteSpace(configuration["AI:Embeddings:Endpoint"])
            ? "aspire-ollama-embedding"
            : string.IsNullOrWhiteSpace(configuration["AI:Embeddings:Model"])
                ? "external-embedding-model"
                : configuration["AI:Embeddings:Model"]!;

        await using var db = await factory.CreateAsync(tenantId, token);
        var chunks = await db.KnowledgeChunks.Where(x => x.DocumentId == documentId && x.IndexStatus != "indexed")
            .OrderBy(x => x.Ordinal).ToListAsync(token);
        var count = 0;
        foreach (var chunk in chunks)
        {
            var numbers = await CreateEmbeddingAsync(chunk.Content, token);
            chunk.Embedding = new Vector(numbers);
            chunk.EmbeddingDimensions = numbers.Length;
            chunk.EmbeddingModel = model;
            chunk.IndexStatus = "indexed";
            chunk.IndexedAt = DateTimeOffset.UtcNow;
            chunk.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            count++;
        }
        return count;
    }

    public async Task<float[]> CreateEmbeddingAsync(string text, CancellationToken token)
    {
        var endpoint = configuration["AI:Embeddings:Endpoint"];
        var model = configuration["AI:Embeddings:Model"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            var nativeVector = await generator.GenerateVectorAsync(text, options: null, cancellationToken: token);
            return ToDatabaseVector(nativeVector.Span);
        }
        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("AI:Embeddings:Model is required when an external embeddings endpoint is configured.");
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("AI:Embeddings:Endpoint must be an HTTPS URL.");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(new { model, input = text })
        };
        var keyReference = configuration["AI:Embeddings:ApiKeyReference"];
        if (!string.IsNullOrWhiteSpace(keyReference))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secrets.Resolve(keyReference));
        using var response = await clients.CreateClient("builders-connectors").SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(token), cancellationToken: token);
        var numbers = json.RootElement.GetProperty("data")[0].GetProperty("embedding")
            .EnumerateArray().Select(x => x.GetSingle()).ToArray();
        return ToDatabaseVector(numbers);
    }

    private static float[] ToDatabaseVector(ReadOnlySpan<float> vector)
    {
        var values = vector.ToArray();
        if (values.Length is < 1 or > 1536 || values.Any(value => !float.IsFinite(value)))
            throw new InvalidOperationException("The embedding provider must return between 1 and 1536 finite values.");
        Array.Resize(ref values, 1536);
        return values;
    }
}
