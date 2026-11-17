using Microsoft.Extensions.Logging;
using SorteFederation.PaymentGateway.Abstractions;

namespace SorteFederation.PaymentGateway.Interfaces;

public sealed class PaymentService(
    IProviderResolver resolver, IPaymentRepository repo, IIdempotencyStore idem, ILogger<PaymentService> log) : IPaymentService
{
    public Task<PaymentResponse> CreateAsync(PaymentRequest req, CancellationToken ct) =>
        idem.ExecuteOnceAsync($"create:{req.TenantId}:{req.IdempotencyKey}", async () =>
        {
            var candidates = await resolver.ResolveCandidatesAsync(req, ct);
            var payment = Payment.Start(req, candidates[0].Provider.Type);
            await repo.AddAsync(payment, ct);

            Exception? last = null;
            foreach (var c in candidates)
            {
                try
                {
                    payment.Reassign(c.Provider.Type);
                    var result = await c.Provider.CreatePaymentAsync(req, c.Credentials, ct);
                    payment.Apply(result);
                    await repo.SaveAsync(payment, ct);
                    return new PaymentResponse(payment.Id, payment.Provider, payment.Status, result.NextAction);
                }
                catch (ProviderUnavailableException ex)
                {
                    last = ex;
                    log.LogWarning(ex, "Provider {Provider} unavailable for payment {PaymentId}, trying next.", c.Provider.Type, payment.Id);
                }
            }

            payment.TryTransition(PaymentStatus.Failed);
            await repo.SaveAsync(payment, ct);
            throw new PaymentException("All payment providers are currently unavailable.", last);
        }, ct);

    public async Task<PaymentResponse> ConfirmAsync(Guid paymentId, string tenantId,
        IReadOnlyDictionary<string, string> data, CancellationToken ct)
    {
        var payment = await GetAsync(paymentId, tenantId, ct);
        if (payment.ProviderRef is null) throw new PaymentValidationException("Payment has not been created at the provider.");

        var (provider, creds) = await resolver.ResolveForTenantAsync(payment.Provider, tenantId, ct);
        var result = await provider.ConfirmAsync(new ConfirmRequest(payment.ProviderRef, data), creds, ct);
        payment.Apply(result);
        await repo.SaveAsync(payment, ct);
        return new PaymentResponse(payment.Id, payment.Provider, payment.Status, result.NextAction);
    }

    public async Task<Payment> GetAsync(Guid paymentId, string tenantId, CancellationToken ct)
    {
        var p = await repo.GetAsync(paymentId, ct);
        // Same error for "missing" and "someone else's" so ids cannot be probed across tenants.
        return p is not null && string.Equals(p.TenantId, tenantId, StringComparison.OrdinalIgnoreCase)
            ? p : throw new PaymentNotFoundException($"Payment {paymentId} not found.");
    }

    public async Task<RefundResponse> RefundAsync(Guid paymentId, string tenantId, decimal? amount, string idempotencyKey, CancellationToken ct)
    {
        var payment = await GetAsync(paymentId, tenantId, ct);
        var refundAmount = new Money(amount ?? payment.Amount.Amount - payment.RefundedTotal, payment.Amount.Currency);

        return await idem.ExecuteOnceAsync($"refund:{tenantId}:{idempotencyKey}", async () =>
        {
            var (provider, creds) = await resolver.ResolveForTenantAsync(payment.Provider, tenantId, ct);
            payment.RegisterRefund(refundAmount);   // validates first; throws before we touch the provider
            var result = await provider.RefundAsync(
                new RefundRequest(payment.ProviderRef!, refundAmount, idempotencyKey, null), creds, ct);
            if (!result.Succeeded) throw new PaymentException("Provider rejected the refund.");
            await repo.SaveAsync(payment, ct);
            return new RefundResponse(result.RefundRef, payment.Status, payment.RefundedTotal);
        }, ct);
    }

    public async Task HandleWebhookAsync(PaymentProviderType type, string tenantId, WebhookContext context, CancellationToken ct)
    {
        var (provider, creds) = await resolver.ResolveForTenantAsync(type, tenantId, ct);
        var ev = await provider.ParseWebhookAsync(context, creds, ct);   // verifies signature

        await idem.ExecuteOnceAsync($"webhook:{type}:{ev.EventId}", async () =>
        {
            if (ev.Kind == WebhookKind.Ignored || ev.ProviderRef is null) return true;

            var payment = await repo.GetByProviderRefAsync(type, ev.ProviderRef, ct);
            if (payment is null) { log.LogWarning("Webhook {Event} for unknown ref {Ref}", ev.EventId, ev.ProviderRef); return true; }

            var target = ev.Kind switch
            {
                WebhookKind.PaymentSucceeded => PaymentStatus.Succeeded,
                WebhookKind.PaymentFailed => PaymentStatus.Failed,
                _ => PaymentStatus.Canceled
            };
            if (payment.TryTransition(target))
            {
                await repo.SaveAsync(payment, ct);
                // TODO(phase 2): post ledger entries + publish PaymentSucceeded through the outbox, same transaction.
            }
            return true;
        }, ct);
    }
}
