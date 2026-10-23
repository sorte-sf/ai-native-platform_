using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using SorteFederation.Builders.Services;
using Microsoft.Extensions.AI;

namespace SorteFederation.Builders.Services;

public sealed record AiCompletionResult(string Provider, string Model, string? Content, long InputTokens, long OutputTokens, bool IsEstimated);

public sealed class AiInferenceService(
    TenantAiPolicyService policies,
    AiUsageMeter usage,
    IHttpClientFactory clients,
    ISecretResolver secrets,
    IConfiguration configuration,
    [FromKeyedServices("chat")] IChatClient localChat,
    ILogger<AiInferenceService> logger)
{
    public async Task<AiCompletionResult> CompleteAsync(Guid tenantId, Guid? applicationId, string feature,
        string systemPrompt, string userPrompt, int? maxOutputTokens, bool jsonResponse, CancellationToken token)
    {
        var policy = await policies.ResolveAsync(tenantId, feature, token);
        if (policy.Mode.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("AI is disabled for this tenant.");

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
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromMinutes(3));
        var response = await localChat.GetResponseAsync(
            [new ChatMessage(ChatRole.System, systemPrompt), new ChatMessage(ChatRole.User, userPrompt)],
            new ChatOptions
            {
                Temperature = 0.1f,
                MaxOutputTokens = maxOutputTokens,
                ResponseFormat = jsonResponse ? ChatResponseFormat.Json : null
            }, timeout.Token);
        return new AiCompletionResult("ollama", policy.Model ?? configuration["AI:Local:Model"] ?? "local",
            response.Text, Convert.ToInt64(response.Usage?.InputTokenCount ?? 0),
            Convert.ToInt64(response.Usage?.OutputTokenCount ?? 0), response.Usage is null);
    }

    private async Task<AiCompletionResult> CompleteCloudAsync(ResolvedAiPolicy policy, string systemPrompt,
        string userPrompt, int maxOutputTokens, bool jsonResponse, CancellationToken token)
    {
        var endpoint = policy.Endpoint;
        var model = policy.Model;
        var keyReference = policy.ApiKeyReference;
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

        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = new[] { new { role = "system", content = systemPrompt }, new { role = "user", content = userPrompt } },
            ["max_tokens"] = maxOutputTokens
        };
        if (jsonResponse) payload["response_format"] = new { type = "json_object" };
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = JsonContent.Create(payload) };
        if (!string.IsNullOrWhiteSpace(keyReference))
        {
            var secret = secrets.Resolve(keyReference);
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
}
