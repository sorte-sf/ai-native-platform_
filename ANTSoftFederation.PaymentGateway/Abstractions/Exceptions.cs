namespace SorteFederation.PaymentGateway.Abstractions;

public class PaymentException(string message, Exception? inner = null) : Exception(message, inner);
public sealed class PaymentValidationException(string message) : PaymentException(message);
public sealed class PaymentConfigurationException(string message) : PaymentException(message);
public sealed class PaymentNotFoundException(string message) : PaymentException(message);
public sealed class WebhookSignatureException(string message) : PaymentException(message);
/// <summary>Transient provider failure (5xx, timeout, 429). The only error that triggers fallback to the next provider.</summary>
public sealed class ProviderUnavailableException(string message, Exception? inner = null) : PaymentException(message, inner);
public sealed class ProviderNotEnabledException(PaymentProviderType? type, string tenant)
    : PaymentException(type is null
        ? $"Tenant '{tenant}' has no enabled payment provider."
        : $"Provider '{type}' is not enabled for tenant '{tenant}'.");
