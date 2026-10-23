namespace SorteFederation.Builders.Services;

public interface ISecretResolver
{
    string Resolve(string reference);
}

public sealed class EnvironmentSecretResolver(IConfiguration configuration) : ISecretResolver
{
    public string Resolve(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) throw new InvalidOperationException("A connector secret reference is required.");
        var value = configuration[$"Secrets:{reference}"] ?? Environment.GetEnvironmentVariable(reference);
        return !string.IsNullOrWhiteSpace(value) ? value : throw new InvalidOperationException($"Secret '{reference}' was not found.");
    }
}

public sealed class VaultOrEnvironmentSecretResolver(IConfiguration configuration, IHttpClientFactory clients) : ISecretResolver
{
    public string Resolve(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) throw new InvalidOperationException("A connector secret reference is required.");
        var address = configuration["Vault:Address"];
        var token = configuration["Vault:Token"] ?? Environment.GetEnvironmentVariable("VAULT_TOKEN");
        if (!string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(token))
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var baseUri) || baseUri.Scheme != Uri.UriSchemeHttps && !baseUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Vault:Address must use HTTPS except for localhost development.");
            var path = configuration[$"Vault:References:{reference}"] ?? reference;
            var uri = new Uri(baseUri, $"v1/{path.TrimStart('/')}");
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("X-Vault-Token", token);
            using var response = clients.CreateClient("builders-connectors").Send(request);
            if (response.IsSuccessStatusCode)
            {
                using var json = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                if (json.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("data", out var values) && values.TryGetProperty("value", out var secret))
                    return secret.GetString() ?? throw new InvalidOperationException($"Vault secret '{reference}' is empty.");
            }
        }
        var fallback = configuration[$"Secrets:{reference}"] ?? Environment.GetEnvironmentVariable(reference);
        return !string.IsNullOrWhiteSpace(fallback) ? fallback : throw new InvalidOperationException($"Secret '{reference}' was not found.");
    }
}
