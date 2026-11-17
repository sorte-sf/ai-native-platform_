using SorteFederation.PaymentGateway.Abstractions;

namespace SorteFederation.PaymentGateway.Interfaces;

/// <summary>Aggregate with an explicit state machine. Illegal or duplicate transitions are rejected, never thrown on
/// the webhook path (duplicate / out-of-order events are normal).</summary>
public sealed class Payment
{
    private static readonly Dictionary<PaymentStatus, PaymentStatus[]> Allowed = new()
    {
        [PaymentStatus.Created]           = [PaymentStatus.RequiresAction, PaymentStatus.Processing, PaymentStatus.Succeeded, PaymentStatus.Failed, PaymentStatus.Canceled],
        [PaymentStatus.RequiresAction]    = [PaymentStatus.Processing, PaymentStatus.Succeeded, PaymentStatus.Failed, PaymentStatus.Canceled],
        [PaymentStatus.Processing]        = [PaymentStatus.Succeeded, PaymentStatus.Failed, PaymentStatus.Canceled],
        [PaymentStatus.Succeeded]         = [PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded],
        [PaymentStatus.PartiallyRefunded] = [PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded],
        [PaymentStatus.Failed]            = [],
        [PaymentStatus.Canceled]          = [],
        [PaymentStatus.Refunded]          = [],
    };

    private Payment() { }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = "";
    public string? OrderId { get; private set; }
    public Money Amount { get; private set; } = new(0, "XXX");
    public decimal RefundedTotal { get; private set; }
    public PaymentProviderType Provider { get; private set; }
    public string? ProviderRef { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Created;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Payment Start(PaymentRequest req, PaymentProviderType provider) => new()
    {
        Id = Guid.NewGuid(), TenantId = req.TenantId, OrderId = req.OrderId, Amount = req.Amount,
        Provider = provider, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
    };

    /// <summary>Only allowed before the provider has accepted anything (used by fallback routing).</summary>
    public void Reassign(PaymentProviderType provider)
    {
        if (Status != PaymentStatus.Created || ProviderRef is not null)
            throw new InvalidOperationException("Provider can only change before the first provider call succeeds.");
        Provider = provider;
    }

    public void Apply(ProviderPaymentResult result)
    {
        ProviderRef = result.ProviderRef;
        TryTransition(result.Status);
    }

    public bool TryTransition(PaymentStatus to)
    {
        if (to == Status) return false;                       // duplicate event: no-op
        if (!Allowed[Status].Contains(to)) return false;      // out-of-order / illegal: ignore
        Status = to; UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }

    public void RegisterRefund(Money refund)
    {
        if (Status is not (PaymentStatus.Succeeded or PaymentStatus.PartiallyRefunded))
            throw new PaymentValidationException($"Cannot refund a payment in status {Status}.");
        if (refund.Currency != Amount.Currency) throw new PaymentValidationException("Currency mismatch.");
        if (refund.Amount <= 0 || RefundedTotal + refund.Amount > Amount.Amount)
            throw new PaymentValidationException("Refund exceeds remaining amount.");

        RefundedTotal += refund.Amount;
        TryTransition(RefundedTotal == Amount.Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded);
    }
}
