using System.Collections.Concurrent;
using SorteFederation.PaymentGateway.Abstractions;

namespace SorteFederation.PaymentGateway.Interfaces;

/// <summary>Dev/test only. Replace with EF Core + Redis (or a DB unique-key table) before production.</summary>
public sealed class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _items = new();
    public Task AddAsync(Payment p, CancellationToken ct) { _items[p.Id] = p; return Task.CompletedTask; }
    public Task SaveAsync(Payment p, CancellationToken ct) { _items[p.Id] = p; return Task.CompletedTask; }
    public Task<Payment?> GetAsync(Guid id, CancellationToken ct) => Task.FromResult(_items.GetValueOrDefault(id));
    public Task<Payment?> GetByProviderRefAsync(PaymentProviderType t, string r, CancellationToken ct) =>
        Task.FromResult(_items.Values.FirstOrDefault(p => p.Provider == t && p.ProviderRef == r));
}

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, Lazy<Task<object?>>> _runs = new();

    public async Task<T> ExecuteOnceAsync<T>(string key, Func<Task<T>> action, CancellationToken ct)
    {
        var lazy = _runs.GetOrAdd(key, _ => new Lazy<Task<object?>>(async () => await action()));
        try { return (T)(await lazy.Value)!; }
        catch { _runs.TryRemove(key, out _); throw; }   // allow retry after failure
    }
}
