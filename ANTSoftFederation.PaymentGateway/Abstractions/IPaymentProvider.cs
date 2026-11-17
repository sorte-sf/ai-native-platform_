namespace SorteFederation.PaymentGateway.Abstractions;

public interface IPaymentProvider
{
    PaymentProviderType Type { get; }
    ProviderCapabilities Capabilities { get; }

    Task<ProviderPaymentResult> CreatePaymentAsync(PaymentRequest request, ProviderCredentials creds, CancellationToken ct);
    /// <summary>PayPal capture, iyzico 3DS completion, Stripe status refresh.</summary>
    Task<ProviderPaymentResult> ConfirmAsync(ConfirmRequest request, ProviderCredentials creds, CancellationToken ct);
    Task<ProviderRefundResult> RefundAsync(RefundRequest request, ProviderCredentials creds, CancellationToken ct);
    /// <summary>Must verify the signature and throw <see cref="WebhookSignatureException"/> if invalid.</summary>
    Task<WebhookEvent> ParseWebhookAsync(WebhookContext context, ProviderCredentials creds, CancellationToken ct);
}
