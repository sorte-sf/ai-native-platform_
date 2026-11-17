using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using SorteFederation.Builders.Services;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;

namespace SorteFederation.Builders.Services;

public sealed record AiCompletionResult(string Provider, string Model, string? Content, long InputTokens, long OutputTokens, bool IsEstimated);

public sealed class AiInferenceTimeoutException(TimeSpan timeout, Exception? innerException = null)
    : TimeoutException($"Local AI generation exceeded its {timeout.TotalMinutes:0}-minute timeout. Increase AI:Local:RequestTimeoutSeconds or use a smaller prompt/model.", innerException);

public static class LocalAiTimeoutOptions
{
    public static TimeSpan GetRequestTimeout(IConfiguration configuration)
    {
        var seconds = int.TryParse(configuration["AI:Local:RequestTimeoutSeconds"], out var configured)
            ? Math.Clamp(configured, 60, 3600)
            : 900;
        return TimeSpan.FromSeconds(seconds);
    }
}

public sealed class AiInferenceService(
    TenantAiPolicyService policies,
    AiUsageMeter usage,
    IHttpClientFactory clients,
    ISecretResolver secrets,
    IConfiguration configuration,
    [FromKeyedServices("chat")] IChatClient localChat,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AiInferenceService> logger)
{
    public async Task<AiCompletionResult> CompleteAsync(Guid tenantId, Guid? applicationId, string feature,
        string systemPrompt, string userPrompt, int? maxOutputTokens, bool jsonResponse, CancellationToken token,
        string? requestedProvider = null, string? requestedModel = null)
    {
        var userId = httpContextAccessor.HttpContext is { } context ? ApplicationEditingPolicy.UserId(context.User) : null;
        var policy = await policies.ResolveAsync(tenantId, feature, token, userId);
        var localModel = configuration["AI:Local:Model"] ?? "qwen3:8b";
        // A user's saved cloud model is an optional alternative; local Ollama remains the default choice.
        if (requestedProvider?.Equals("ollama", StringComparison.OrdinalIgnoreCase) == true &&
            requestedModel?.Equals(localModel, StringComparison.OrdinalIgnoreCase) == true &&
            !policy.Mode.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            var workspacePolicy = await policies.ResolveAsync(tenantId, feature, token);
            if (!workspacePolicy.Mode.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                policy = workspacePolicy with { Mode = "Local", Provider = "ollama", Endpoint = null, Model = localModel, ApiKeyReference = null, ApiKey = null };
        }
        if (policy.Mode.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("AI is disabled for this tenant.");

        if (!string.IsNullOrWhiteSpace(requestedProvider) || !string.IsNullOrWhiteSpace(requestedModel))
        {
            if (!policy.Mode.Equals("CustomerCloud", StringComparison.OrdinalIgnoreCase) &&
                !policy.Mode.Equals("Local", StringComparison.OrdinalIgnoreCase) &&
                !policy.Mode.Equals("PlatformManaged", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The selected AI provider is not available for this workspace.");
            if (!string.IsNullOrWhiteSpace(requestedProvider) && !requestedProvider.Equals(policy.Provider, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Choose a provider configured for this workspace first.");
            if (!string.IsNullOrWhiteSpace(requestedModel) &&
                !requestedModel.Equals(policy.Model ?? (policy.Mode.Equals("Local", StringComparison.OrdinalIgnoreCase) ? localModel : null), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The selected model is not configured for this account.");
        }

        var maximumOutput = Math.Clamp(maxOutputTokens ?? policy.MaxOutputTokens, 1, policy.MaxOutputTokens);
        var inputText = systemPrompt + "\n" + userPrompt;
        var estimatedInput = AiUsageMeter.EstimateTokens(inputText);
        AiUsageReservation? reservation = null;
        if (policy.Mode.Equals("PlatformManaged", StringComparison.OrdinalIgnoreCase))
            reservation = await usage.ReserveAsync(policy, estimatedInput, token);

        var timer = Stopwatch.StartNew();
        try
        {
            var result = policy.Mode.Equals("Local", StringComparison.OrdinalIgnoreCase)
                ? await CompleteLocalAsync(policy, systemPrompt, userPrompt, maximumOutput, jsonResponse, token)
                : await CompleteCloudAsync(policy, systemPrompt, userPrompt, maximumOutput, jsonResponse, token);
            timer.Stop();

            var inputTokens = result.InputTokens > 0 ? result.InputTokens : estimatedInput;

            var outputTokens = result.OutputTokens > 0 ? result.OutputTokens : AiUsageMeter.EstimateTokens(result.Content ?? "");

            var estimated = result.IsEstimated || result.InputTokens <= 0 || result.OutputTokens <= 0;

            await usage.RecordAsync(policy, reservation, applicationId, feature, result.Provider, result.Model, inputTokens,
                outputTokens, estimated, "completed", null, timer.ElapsedMilliseconds, token);

            return result with { InputTokens = inputTokens, OutputTokens = outputTokens, IsEstimated = estimated };
        }
        catch (Exception exception)
        {
            timer.Stop();
            try
            {
                await usage.RecordAsync(policy, reservation, applicationId, feature, policy.Provider, policy.Model ?? "unknown",
                    estimatedInput, 0, true, "failed", exception.GetType().Name, timer.ElapsedMilliseconds, CancellationToken.None);
            }
            catch (Exception auditException)
            {
                logger.LogError(auditException, "Could not persist failed AI usage for tenant {TenantId}.", tenantId);
            }
            throw;
        }
    }

    private async Task<AiCompletionResult> CompleteLocalAsync(ResolvedAiPolicy policy, string systemPrompt,
        string userPrompt, int maxOutputTokens, bool jsonResponse, CancellationToken token)
    {
        var requestTimeout = LocalAiTimeoutOptions.GetRequestTimeout(configuration);
        var timer = Stopwatch.StartNew();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(requestTimeout);
        try
        {
            var chatOptions = new ChatOptions
            {
                Temperature = 0.1f,
                MaxOutputTokens = maxOutputTokens,
                ResponseFormat = jsonResponse ? ChatResponseFormat.Json : null
            };
            // Qwen3 defaults to reasoning mode. Structured generation can exhaust
            // its output budget on hidden reasoning before producing the requested JSON.
            if (jsonResponse)
                chatOptions.AddOllamaOption(OllamaOption.Think, false);

            var response = await localChat.GetResponseAsync(
                [new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt)],
                chatOptions, timeout.Token);
            return new AiCompletionResult("ollama", policy.Model ?? configuration["AI:Local:Model"] ?? "qwen3:8b",
                response.Text, Convert.ToInt64(response.Usage?.InputTokenCount ?? 0),
                Convert.ToInt64(response.Usage?.OutputTokenCount ?? 0), response.Usage is null);
        }
        catch (OperationCanceledException exception) when (!token.IsCancellationRequested)
        {
            logger.LogWarning(exception,
                "Local Ollama inference for tenant {TenantId} using model {Model} exceeded {TimeoutSeconds} seconds after {ElapsedSeconds} seconds.",
                policy.TenantId, policy.Model ?? configuration["AI:Local:Model"] ?? "qwen3:8b",
                requestTimeout.TotalSeconds, timer.Elapsed.TotalSeconds);
            throw new AiInferenceTimeoutException(requestTimeout, exception);
        }
    }

    private async Task<AiCompletionResult> CompleteCloudAsync(ResolvedAiPolicy policy, string systemPrompt,
        string userPrompt, int maxOutputTokens, bool jsonResponse, CancellationToken token)
    {
        var endpoint = policy.Endpoint;
        var model = policy.Model;
        var keyReference = policy.ApiKeyReference;
        var personalKey = policy.ApiKey;
        var authScheme = policy.AuthScheme;
        if (policy.Mode.Equals("PlatformManaged", StringComparison.OrdinalIgnoreCase))
        {
            endpoint ??= configuration["AI:Endpoint"];
            model ??= configuration["AI:Model"];
            keyReference ??= configuration["AI:ApiKeyReference"];
            authScheme = configuration["AI:AuthScheme"] ?? authScheme;
        }
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(uri.UserInfo) || string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("The configured cloud AI provider needs an HTTPS endpoint and a model.");
        await EnsurePublicEndpointAsync(uri, token);

        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = new[] { new { role = "system", content = systemPrompt }, new { role = "user", content = userPrompt } },
            ["max_tokens"] = maxOutputTokens
        };
        if (jsonResponse) payload["response_format"] = new { type = "json_object" };
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = JsonContent.Create(payload) };
        if (!string.IsNullOrWhiteSpace(keyReference) || !string.IsNullOrWhiteSpace(personalKey))
        {
            var secret = personalKey ?? secrets.Resolve(keyReference!);
            if (authScheme.Equals("api-key", StringComparison.OrdinalIgnoreCase))
                request.Headers.TryAddWithoutValidation("X-API-Key", secret);
            else request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
        }

        using var response = await clients.CreateClient("builders-connectors").SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(token), cancellationToken: token);
        var root = json.RootElement;
        var content = root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0 &&
                      choices[0].TryGetProperty("message", out var message) && message.TryGetProperty("content", out var value)
            ? value.GetString()
            : root.TryGetProperty("content", out var direct) ? direct.GetString() : null;
        var inputTokens = ReadUsage(root, "prompt_tokens", "input_tokens");
        var outputTokens = ReadUsage(root, "completion_tokens", "output_tokens");
        return new AiCompletionResult(policy.Provider.Length == 0 ? "openai-compatible" : policy.Provider,
            model, content, inputTokens, outputTokens, inputTokens == 0 || outputTokens == 0);
    }

    private static long ReadUsage(JsonElement root, string primary, string fallback) =>
        root.TryGetProperty("usage", out var usage) &&
        (usage.TryGetProperty(primary, out var count) || usage.TryGetProperty(fallback, out count)) &&
        count.TryGetInt64(out var value) ? value : 0;

    private static async Task EnsurePublicEndpointAsync(Uri uri, CancellationToken token)
    {
        var addresses = System.Net.IPAddress.TryParse(uri.DnsSafeHost, out var literal)
            ? [literal]
            : await System.Net.Dns.GetHostAddressesAsync(uri.DnsSafeHost, token);
        if (addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
            throw new InvalidOperationException("The AI endpoint must resolve only to public network addresses.");
    }

    public static bool IsPublicAddress(System.Net.IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        if (System.Net.IPAddress.IsLoopback(address) || address.Equals(System.Net.IPAddress.Any) ||
            address.Equals(System.Net.IPAddress.IPv6Any) || address.Equals(System.Net.IPAddress.IPv6None) ||
            address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast) return false;
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            var bytes = address.GetAddressBytes();
            return (bytes[0] & 0xfe) != 0xfc; // block unique-local fc00::/7
        }
        if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;
        var b = address.GetAddressBytes();
        return b[0] != 0 && b[0] != 10 && b[0] != 127 && b[0] < 224 &&
            !(b[0] == 100 && b[1] is >= 64 and <= 127) &&
            !(b[0] == 169 && b[1] == 254) &&
            !(b[0] == 172 && b[1] is >= 16 and <= 31) &&
            !(b[0] == 192 && b[1] == 168) &&
            !(b[0] == 198 && b[1] is 18 or 19);
    }
}
