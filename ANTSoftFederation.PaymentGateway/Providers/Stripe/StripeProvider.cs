using SorteFederation.PaymentGateway.Abstractions;
using SorteFederation.PaymentGateway.Interfaces;
using Stripe;

namespace SorteFederation.PaymentGateway.Providers.Stripe;

public sealed class StripeProvider : IPaymentProvider
{
    public PaymentProviderType Type => PaymentProviderType.Stripe;
    public ProviderCapabilities Capabilities =>
        ProviderCapabilities.Cards | ProviderCapabilities.Wallets | ProviderCapabilities.Refunds |
        ProviderCapabilities.PartialRefunds | ProviderCapabilities.ThreeDS | ProviderCapabilities.Subscriptions;

    // Credentials: SecretKey, PublishableKey, WebhookSecret

    public async Task<ProviderPaymentResult> CreatePaymentAsync(PaymentRequest req, ProviderCredentials creds, CancellationToken ct)
    {
        var metadata = new Dictionary<string, string>(req.Metadata) { ["tenant"] = req.TenantId };
        if (req.OrderId is not null) metadata["order"] = req.OrderId;

        var options = new PaymentIntentCreateOptions
        {
            Amount = CurrencyMath.ToMinorUnits(req.Amount),
            Currency = req.Amount.Currency.ToLowerInvariant(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            ReceiptEmail = req.Customer.Email,
            Metadata = metadata,
        };
        if (req.ThreeDS == ThreeDSMode.Required)
            options.PaymentMethodOptions = new PaymentIntentPaymentMethodOptionsOptions
            { Card = new PaymentIntentPaymentMethodOptionsCardOptions { RequestThreeDSecure = "any" } };

        try
        {
            var pi = await Intents(creds).CreateAsync(options,
                new RequestOptions { IdempotencyKey = $"{req.TenantId}:{req.IdempotencyKey}" }, ct);
            return new ProviderPaymentResult(pi.Id, MapStatus(pi.Status),
                new NextAction.ClientSecret(pi.ClientSecret, creds.Get("PublishableKey")));
        }
        catch (StripeException ex) { throw Translate(ex); }
    }

    public async Task<ProviderPaymentResult> ConfirmAsync(ConfirmRequest req, ProviderCredentials creds, CancellationToken ct)
    {
        try
        {
            var pi = await Intents(creds).GetAsync(req.ProviderRef, cancellationToken: ct);
            return new ProviderPaymentResult(pi.Id, MapStatus(pi.Status), new NextAction.None());
        }
        catch (StripeException ex) { throw Translate(ex); }
    }

    public async Task<ProviderRefundResult> RefundAsync(RefundRequest req, ProviderCredentials creds, CancellationToken ct)
    {
        try
        {
            var refund = await new RefundService(Client(creds)).CreateAsync(
                new RefundCreateOptions
                {
                    PaymentIntent = req.ProviderRef,
                    Amount = req.Amount is null ? null : CurrencyMath.ToMinorUnits(req.Amount),
                },
                new RequestOptions { IdempotencyKey = $"refund:{req.IdempotencyKey}" }, ct);
            return new ProviderRefundResult(refund.Id, refund.Status is "succeeded" or "pending", req.Amount is not null);
        }
        catch (StripeException ex) { throw Translate(ex); }
    }

    public Task<WebhookEvent> ParseWebhookAsync(WebhookContext ctx, ProviderCredentials creds, CancellationToken ct)
    {
        Event ev;
        try
        {
            ev = EventUtility.ConstructEvent(ctx.Payload, ctx.Headers.GetValueOrDefault("Stripe-Signature") ?? "",
                creds.Get("WebhookSecret"), throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex) { throw new WebhookSignatureException(ex.Message); }

        var kind = ev.Type switch
        {
            "payment_intent.succeeded" => WebhookKind.PaymentSucceeded,
            "payment_intent.payment_failed" => WebhookKind.PaymentFailed,
            "payment_intent.canceled" => WebhookKind.PaymentCanceled,
            _ => WebhookKind.Ignored
        };
        return Task.FromResult(new WebhookEvent(ev.Id, kind, (ev.Data.Object as PaymentIntent)?.Id));
    }

    private static StripeClient Client(ProviderCredentials c) => new(c.Get("SecretKey"));
    private static PaymentIntentService Intents(ProviderCredentials c) => new(Client(c));

    private static PaymentStatus MapStatus(string s) => s switch
    {
        "succeeded" => PaymentStatus.Succeeded,
        "canceled" => PaymentStatus.Canceled,
        "processing" => PaymentStatus.Processing,
        _ => PaymentStatus.RequiresAction   // requires_payment_method / requires_action / requires_confirmation / requires_capture
    };

    private static PaymentException Translate(StripeException ex) =>
        ex.HttpStatusCode == 0 || (int)ex.HttpStatusCode >= 500 || (int)ex.HttpStatusCode == 429
            ? new ProviderUnavailableException("Stripe is unavailable.", ex)
            : new PaymentException($"Stripe error: {ex.Message}", ex);
}

public static class StripeExtensions
{
    public static PaymentPlatformBuilder AddStripe(this PaymentPlatformBuilder b) =>
        b.AddProvider<StripeProvider>(PaymentProviderType.Stripe);
}
