using SorteFederation.PaymentGateway.Abstractions;

namespace SorteFederation.PaymentGateway.Interfaces;

public sealed record PaymentResponse(Guid PaymentId, PaymentProviderType Provider, PaymentStatus Status, NextAction NextAction);
public sealed record RefundResponse(string RefundRef, PaymentStatus PaymentStatus, decimal RefundedTotal);
public sealed record ResolvedProvider(IPaymentProvider Provider, ProviderCredentials Credentials);

public interface IPaymentService
{
    Task<PaymentResponse> CreateAsync(PaymentRequest request, CancellationToken ct);
    Task<PaymentResponse> ConfirmAsync(Guid paymentId, string tenantId, IReadOnlyDictionary<string, string> data, CancellationToken ct);
    Task<Payment> GetAsync(Guid paymentId, string tenantId, CancellationToken ct);
    Task<RefundResponse> RefundAsync(Guid paymentId, string tenantId, decimal? amount, string idempotencyKey, CancellationToken ct);
    Task HandleWebhookAsync(PaymentProviderType provider, string tenantId, WebhookContext context, CancellationToken ct);
}

public interface IProviderResolver
{
    /// <summary>Ordered candidates: first is primary, the rest are fallbacks (explicit provider = no fallbacks).</summary>
    Task<IReadOnlyList<ResolvedProvider>> ResolveCandidatesAsync(PaymentRequest request, CancellationToken ct);
    Task<ResolvedProvider> ResolveForTenantAsync(PaymentProviderType type, string tenantId, CancellationToken ct);
}

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken ct);
    Task SaveAsync(Payment payment, CancellationToken ct);
    Task<Payment?> GetAsync(Guid id, CancellationToken ct);
    Task<Payment?> GetByProviderRefAsync(PaymentProviderType provider, string providerRef, CancellationToken ct);
}

public interface IIdempotencyStore
{
    /// <summary>Runs <paramref name="action"/> once per key; concurrent/repeated callers get the same result.
    /// Failed runs are not cached so the client can retry with the same key.</summary>
    Task<T> ExecuteOnceAsync<T>(string key, Func<Task<T>> action, CancellationToken ct);
}
