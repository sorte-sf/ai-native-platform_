namespace SorteFederation.Builders.Services;

public sealed record AiGatewayResponse(string Provider, string? Content, IReadOnlyList<object> Sources);

public sealed class AiGatewayService(
    KnowledgeService knowledge,
    TenantAiPolicyService policies,
    AiInferenceService inference,
    AuditService audit,
    IHttpContextAccessor accessor)
{
    public async Task<AiGatewayResponse> CompleteAsync(AiPromptRequest request, CancellationToken token)
    {
        if (request.TenantId == Guid.Empty || string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Tenant and prompt are required.");
        if (request.Prompt.Length > 32_000 || request.KnowledgeQuery?.Length > 2_000)
            throw new ArgumentException("Prompt or knowledge query exceeds the supported size limit.");

        var documents = request.KnowledgeQuery is null
            ? [] : await knowledge.SearchAsync(new KnowledgeSearchRequest(request.TenantId, request.KnowledgeQuery), token);
        var sources = documents.Select(x => (object)new { x.Id, x.Title, x.Source }).ToList();
        var policy = await policies.ResolveAsync(request.TenantId, "knowledge-chat", token);
        if (policy.Mode.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
        {
            await RecordAudit(request.TenantId, "disabled", sources.Count, "disabled", token);
            return new AiGatewayResponse("disabled", null, sources);
        }

        var context = string.Join("\n\n", documents.Select(x => $"[{x.Title}]\n{x.Content}"));
        if (context.Length > 120_000) context = context[..120_000];
        var prompt = string.IsNullOrWhiteSpace(context)
            ? request.Prompt : $"Knowledge sources:\n{context}\n\nRequest:\n{request.Prompt}";
        var result = await inference.CompleteAsync(request.TenantId, null, "knowledge-chat",
            "Answer using the supplied tenant knowledge sources when relevant. Do not invent facts.", prompt,
            null, false, token);
        await RecordAudit(request.TenantId, result.Provider, sources.Count, "completed", token);
        return new AiGatewayResponse(result.Provider, result.Content, sources);
    }

    private Task RecordAudit(Guid tenantId, string provider, int sourceCount, string outcome, CancellationToken token) =>
        audit.Record(new CreateAuditEventRequest(tenantId, "ai.request.completed", "ai", null,
            JsonSerializer.Serialize(new { provider, sourceCount, outcome })),
            accessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal(), accessor.HttpContext?.TraceIdentifier, token);
}
