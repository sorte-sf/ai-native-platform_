using System.Globalization;
using System.Text.Json.Serialization;

namespace SorteFederation.PaymentGateway.Abstractions;

public enum PaymentProviderType { Stripe, Iyzico, PayPal }
public enum PaymentMethod { Card, Wallet }
public enum ThreeDSMode { Auto, Required, Disabled }
public enum PaymentStatus { Created, RequiresAction, Processing, Succeeded, Failed, Canceled, PartiallyRefunded, Refunded }
public enum WebhookKind { PaymentSucceeded, PaymentFailed, PaymentCanceled, Ignored }

[Flags]
public enum ProviderCapabilities
{
    None = 0, Cards = 1, Wallets = 2, Refunds = 4, PartialRefunds = 8,
    ThreeDS = 16, Subscriptions = 32, Installments = 64
}

public sealed record Money(decimal Amount, string Currency);
public sealed record LineItem(string Name, int Qty, decimal UnitPrice);
public sealed record CustomerInfo(string? Email = null, string? FirstName = null, string? LastName = null,
                                  string? Country = null, string? Ip = null);

/// <summary>Per-tenant secrets for one provider (resolved at runtime, never cached in singletons).</summary>
public sealed class ProviderCredentials(IReadOnlyDictionary<string, string> values)
{
    public string Get(string key) =>
        values.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v : throw new PaymentConfigurationException($"Missing credential '{key}'.");
    public string? GetOrNull(string key) => values.GetValueOrDefault(key);
}

/// <summary>What the client must do next. Angular switches on "type", never on provider.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(None), "none")]
[JsonDerivedType(typeof(ClientSecret), "clientSecret")]
[JsonDerivedType(typeof(Redirect), "redirect")]
[JsonDerivedType(typeof(HtmlContent), "html")]
public abstract record NextAction
{
    private NextAction() { }
    public sealed record None : NextAction;
    public sealed record ClientSecret(string Secret, string PublishableKey) : NextAction;  // Stripe
    public sealed record Redirect(string Url) : NextAction;                                 // PayPal
    public sealed record HtmlContent(string Html) : NextAction;                             // iyzico 3DS / checkout form
}

public sealed record PaymentRequest(
    string TenantId, string? OrderId, Money Amount, PaymentProviderType? Provider, PaymentMethod Method,
    ThreeDSMode ThreeDS, CustomerInfo Customer, IReadOnlyList<LineItem> Items,
    string? ReturnUrl, string? CancelUrl, string IdempotencyKey, IReadOnlyDictionary<string, string> Metadata)
{
    public static PaymentRequestBuilder Create() => new();
}

public sealed record ConfirmRequest(string ProviderRef, IReadOnlyDictionary<string, string> Data);
public sealed record RefundRequest(string ProviderRef, Money? Amount, string IdempotencyKey, string? Reason);

public sealed record ProviderPaymentResult(string ProviderRef, PaymentStatus Status, NextAction NextAction);
public sealed record ProviderRefundResult(string RefundRef, bool Succeeded, bool Partial);

public sealed record WebhookContext(string Payload, IReadOnlyDictionary<string, string> Headers);
public sealed record WebhookEvent(string EventId, WebhookKind Kind, string? ProviderRef);

public static class CurrencyMath
{
    private static readonly HashSet<string> Zero = ["JPY", "KRW", "VND", "CLP", "ISK", "UGX", "XAF", "XOF"];
    private static readonly HashSet<string> Three = ["BHD", "KWD", "OMR", "JOD", "TND"];

    public static int DecimalPlaces(string currency) =>
        Zero.Contains(currency.ToUpperInvariant()) ? 0 : Three.Contains(currency.ToUpperInvariant()) ? 3 : 2;

    public static long ToMinorUnits(Money m)
    {
        var factor = 1m;
        for (var i = 0; i < DecimalPlaces(m.Currency); i++) factor *= 10m;
        return (long)Math.Round(m.Amount * factor, MidpointRounding.AwayFromZero);
    }

    public static string Format(Money m) =>
        m.Amount.ToString("F" + DecimalPlaces(m.Currency), CultureInfo.InvariantCulture);
}
