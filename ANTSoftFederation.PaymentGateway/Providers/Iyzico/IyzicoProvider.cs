using SorteFederation.PaymentGateway.Abstractions;
using SorteFederation.PaymentGateway.Interfaces;

namespace SorteFederation.PaymentGateway.Providers.Iyzico;

/// <summary>
/// SKELETON. Registered and routable, but not implemented yet; I will build it against the official Iyzipay SDK.
/// Credentials: ApiKey, SecretKey, BaseUrl ("https://sandbox-api.iyzipay.com" | "https://api.iyzipay.com").
/// Plan:
///  CreatePaymentAsync -> ThreedsInitialize (or Checkout Form initialize); return NextAction.HtmlContent(threeDSHtmlContent).
///                        Basket items must sum to price; amounts are decimal strings; conversationId = idempotency key.
///  ConfirmAsync       -> iyzico POSTs the bank result to our callback; call ThreedsPayment.Create with paymentId + conversationData.
///  RefundAsync        -> Refund (per payment transaction id) or Cancel (same day, full).
///  ParseWebhookAsync  -> verify X-IYZ-SIGNATURE-V3 (HMAC-SHA256 over secretKey + event fields), then map status.
/// </summary>
public sealed class IyzicoProvider : IPaymentProvider
{
    public PaymentProviderType Type => PaymentProviderType.Iyzico;
    public ProviderCapabilities Capabilities =>
        ProviderCapabilities.Cards | ProviderCapabilities.ThreeDS | ProviderCapabilities.Refunds |
        ProviderCapabilities.PartialRefunds | ProviderCapabilities.Installments;

    public Task<ProviderPaymentResult> CreatePaymentAsync(PaymentRequest r, ProviderCredentials c, CancellationToken ct) => NotYet();
    public Task<ProviderPaymentResult> ConfirmAsync(ConfirmRequest r, ProviderCredentials c, CancellationToken ct) => NotYet();
    public Task<ProviderRefundResult> RefundAsync(RefundRequest r, ProviderCredentials c, CancellationToken ct) => NotYet<ProviderRefundResult>();
    public Task<WebhookEvent> ParseWebhookAsync(WebhookContext x, ProviderCredentials c, CancellationToken ct) => NotYet<WebhookEvent>();

    private static Task<ProviderPaymentResult> NotYet() => NotYet<ProviderPaymentResult>();
    private static Task<T> NotYet<T>() => throw new NotImplementedException("iyzico adapter is not implemented yet.");
}

public static class IyzicoExtensions
{
    public static PaymentPlatformBuilder AddIyzico(this PaymentPlatformBuilder b) =>
        b.AddProvider<IyzicoProvider>(PaymentProviderType.Iyzico);
}
