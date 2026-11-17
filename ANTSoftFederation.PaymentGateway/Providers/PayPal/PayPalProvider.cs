using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using SorteFederation.PaymentGateway.Abstractions;
using SorteFederation.PaymentGateway.Interfaces;

namespace SorteFederation.PaymentGateway.Providers.PayPal;

/// <summary>PayPal Orders v2 over HttpClient. Credentials: ClientId, ClientSecret, WebhookId, Mode ("sandbox"|"live").</summary>
public sealed class PayPalProvider(IHttpClientFactory http) : IPaymentProvider
{
    private static readonly ConcurrentDictionary<string, (string Token, DateTimeOffset Expires)> Tokens = new();

    public PaymentProviderType Type => PaymentProviderType.PayPal;
    public ProviderCapabilities Capabilities =>
        ProviderCapabilities.Wallets | ProviderCapabilities.Refunds | ProviderCapabilities.PartialRefunds;

    public async Task<ProviderPaymentResult> CreatePaymentAsync(PaymentRequest req, ProviderCredentials creds, CancellationToken ct)
    {
        var client = await ClientAsync(creds, ct);
        var body = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = req.OrderId ?? "default",
                    custom_id = req.TenantId,
                    amount = new { currency_code = req.Amount.Currency, value = CurrencyMath.Format(req.Amount) }
                }
            },
            payment_source = new
            {
                paypal = new
                {
                    experience_context = new { return_url = req.ReturnUrl, cancel_url = req.CancelUrl, user_action = "PAY_NOW" }
                }
            }
        };
        var order = await SendAsync(client, HttpMethod.Post, "/v2/checkout/orders", body, $"{req.TenantId}:{req.IdempotencyKey}", ct);

        var link = order["links"]?.AsArray()
            .FirstOrDefault(l => l?["rel"]?.GetValue<string>() is "payer-action" or "approve")?["href"]?.GetValue<string>()
            ?? throw new PaymentException("PayPal did not return an approval link.");

        return new ProviderPaymentResult(order["id"]!.GetValue<string>(), PaymentStatus.RequiresAction, new NextAction.Redirect(link));
    }

    /// <summary>Called when the buyer returns from PayPal: captures the approved order.</summary>
    public async Task<ProviderPaymentResult> ConfirmAsync(ConfirmRequest req, ProviderCredentials creds, CancellationToken ct)
    {
        var client = await ClientAsync(creds, ct);
        JsonNode order;
        try { order = await SendAsync(client, HttpMethod.Post, $"/v2/checkout/orders/{req.ProviderRef}/capture", new { }, null, ct); }
        catch (PaymentException ex) when (ex.Message.Contains("ORDER_ALREADY_CAPTURED"))
        { order = await SendAsync(client, HttpMethod.Get, $"/v2/checkout/orders/{req.ProviderRef}", null, null, ct); }

        var status = order["status"]?.GetValue<string>() switch
        {
            "COMPLETED" => PaymentStatus.Succeeded,
            "VOIDED" => PaymentStatus.Canceled,
            "APPROVED" or "PAYER_ACTION_REQUIRED" or "CREATED" => PaymentStatus.RequiresAction,
            _ => PaymentStatus.Processing
        };
        return new ProviderPaymentResult(req.ProviderRef, status, new NextAction.None());
    }

    public async Task<ProviderRefundResult> RefundAsync(RefundRequest req, ProviderCredentials creds, CancellationToken ct)
    {
        var client = await ClientAsync(creds, ct);
        var order = await SendAsync(client, HttpMethod.Get, $"/v2/checkout/orders/{req.ProviderRef}", null, null, ct);
        var captureId = order["purchase_units"]?[0]?["payments"]?["captures"]?[0]?["id"]?.GetValue<string>()
            ?? throw new PaymentException("No capture found for this PayPal order.");

        object body = req.Amount is null ? new { }
            : new { amount = new { currency_code = req.Amount.Currency, value = CurrencyMath.Format(req.Amount) } };
        var refund = await SendAsync(client, HttpMethod.Post, $"/v2/payments/captures/{captureId}/refund", body, $"refund:{req.IdempotencyKey}", ct);

        var status = refund["status"]?.GetValue<string>();
        return new ProviderRefundResult(refund["id"]!.GetValue<string>(), status is "COMPLETED" or "PENDING", req.Amount is not null);
    }

    public async Task<WebhookEvent> ParseWebhookAsync(WebhookContext ctx, ProviderCredentials creds, CancellationToken ct)
    {
        string H(string name) => ctx.Headers.GetValueOrDefault(name) ?? "";
        var evt = JsonNode.Parse(ctx.Payload) ?? throw new WebhookSignatureException("Invalid payload.");

        var client = await ClientAsync(creds, ct);
        var verify = new JsonObject
        {
            ["auth_algo"] = H("PayPal-Auth-Algo"),
            ["cert_url"] = H("PayPal-Cert-Url"),
            ["transmission_id"] = H("PayPal-Transmission-Id"),
            ["transmission_sig"] = H("PayPal-Transmission-Sig"),
            ["transmission_time"] = H("PayPal-Transmission-Time"),
            ["webhook_id"] = creds.Get("WebhookId"),
            ["webhook_event"] = JsonNode.Parse(ctx.Payload),
        };
        var res = await SendAsync(client, HttpMethod.Post, "/v1/notifications/verify-webhook-signature", verify, null, ct);
        if (res["verification_status"]?.GetValue<string>() != "SUCCESS")
            throw new WebhookSignatureException("PayPal webhook verification failed.");

        var kind = evt["event_type"]?.GetValue<string>() switch
        {
            "PAYMENT.CAPTURE.COMPLETED" => WebhookKind.PaymentSucceeded,
            "PAYMENT.CAPTURE.DENIED" or "PAYMENT.CAPTURE.DECLINED" => WebhookKind.PaymentFailed,
            _ => WebhookKind.Ignored
        };
        var orderId = evt["resource"]?["supplementary_data"]?["related_ids"]?["order_id"]?.GetValue<string>();
        return new WebhookEvent(evt["id"]!.GetValue<string>(), kind, orderId);
    }

    // ---- HTTP plumbing ----

    private async Task<HttpClient> ClientAsync(ProviderCredentials c, CancellationToken ct)
    {
        var client = http.CreateClient("paypal");
        client.BaseAddress = new Uri(c.GetOrNull("Mode") == "live" ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com");

        var cacheKey = $"{client.BaseAddress}|{c.Get("ClientId")}";
        if (!Tokens.TryGetValue(cacheKey, out var t) || t.Expires <= DateTimeOffset.UtcNow.AddMinutes(1))
        {
            using var rq = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token")
            { Content = new FormUrlEncodedContent([new KeyValuePair<string, string>("grant_type", "client_credentials")]) };
            rq.Headers.Authorization = new("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{c.Get("ClientId")}:{c.Get("ClientSecret")}")));
            using var rs = await client.SendAsync(rq, ct);
            var text = await rs.Content.ReadAsStringAsync(ct);
            if (!rs.IsSuccessStatusCode) throw Fail(rs, text);
            var json = JsonNode.Parse(text)!;
            t = (json["access_token"]!.GetValue<string>(), DateTimeOffset.UtcNow.AddSeconds(json["expires_in"]!.GetValue<int>()));
            Tokens[cacheKey] = t;
        }
        client.DefaultRequestHeaders.Authorization = new("Bearer", t.Token);
        return client;
    }

    private static async Task<JsonNode> SendAsync(HttpClient client, HttpMethod method, string url, object? body, string? requestId, CancellationToken ct)
    {
        using var rq = new HttpRequestMessage(method, url);
        if (body is not null) rq.Content = JsonContent.Create(body, body.GetType());
        if (requestId is not null) rq.Headers.Add("PayPal-Request-Id", requestId);
        using var rs = await client.SendAsync(rq, ct);
        var text = await rs.Content.ReadAsStringAsync(ct);
        if (!rs.IsSuccessStatusCode) throw Fail(rs, text);
        return JsonNode.Parse(text) ?? new JsonObject();
    }

    private static PaymentException Fail(HttpResponseMessage rs, string body) =>
        (int)rs.StatusCode >= 500 || (int)rs.StatusCode == 429
            ? new ProviderUnavailableException($"PayPal {(int)rs.StatusCode}.")
            : new PaymentException($"PayPal error {(int)rs.StatusCode}: {body}");
}

public static class PayPalExtensions
{
    public static PaymentPlatformBuilder AddPayPal(this PaymentPlatformBuilder b)
    {
        b.Services.AddHttpClient("paypal").AddStandardResilienceHandler(o =>
        {
            o.Retry.DisableForUnsafeHttpMethods();   // POSTs are retried only where we pass PayPal-Request-Id, i.e. by our own layer
            o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(25);
            o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);   // must be >= 2x attempt timeout
        });
        return b.AddProvider<PayPalProvider>(PaymentProviderType.PayPal);
    }
}
