using OllamaSharp;

namespace SorteFederation.Builders.Services.Ollama;

public class OllamaService(
[FromKeyedServices("chat")] IOllamaApiClient chatOllama,
[FromKeyedServices("embeddings")] IOllamaApiClient embeddingsOllama)
{
    // Use ollama clients...
}