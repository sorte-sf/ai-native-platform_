namespace SorteFederation.PaymentGateway.Abstractions;

public sealed class CustomerBuilder
{
    private string? _email, _first, _last, _country, _ip;
    public CustomerBuilder Email(string email) { _email = email; return this; }
    public CustomerBuilder Name(string first, string last) { _first = first; _last = last; return this; }
    public CustomerBuilder Country(string iso2) { _country = iso2.ToUpperInvariant(); return this; }
    public CustomerBuilder Ip(string? ip) { _ip = ip; return this; }
    internal CustomerInfo Build() => new(_email, _first, _last, _country, _ip);
}

public sealed class PaymentRequestBuilder
{
    private string? _tenantId, _orderId, _idempotencyKey, _returnUrl, _cancelUrl;
    private Money? _amount;
    private PaymentProviderType? _provider;
    private PaymentMethod _method = PaymentMethod.Card;
    private ThreeDSMode _threeDs = ThreeDSMode.Auto;
    private CustomerInfo _customer = new();
    private readonly List<LineItem> _items = [];
    private readonly Dictionary<string, string> _metadata = [];

    public PaymentRequestBuilder ForTenant(string tenantId) { _tenantId = tenantId; return this; }
    public PaymentRequestBuilder ForOrder(string orderId) { _orderId = orderId; return this; }
    public PaymentRequestBuilder WithAmount(decimal amount, string currency)
    { _amount = new Money(amount, currency.ToUpperInvariant()); return this; }
    public PaymentRequestBuilder UseProvider(PaymentProviderType? provider) { _provider = provider; return this; }
    public PaymentRequestBuilder WithMethod(PaymentMethod method) { _method = method; return this; }
    public PaymentRequestBuilder WithThreeDS(ThreeDSMode mode) { _threeDs = mode; return this; }
    public PaymentRequestBuilder WithIdempotencyKey(string key) { _idempotencyKey = key; return this; }
    public PaymentRequestBuilder WithReturnUrls(string success, string cancel) { _returnUrl = success; _cancelUrl = cancel; return this; }
    public PaymentRequestBuilder WithMetadata(string key, string value) { _metadata[key] = value; return this; }
    public PaymentRequestBuilder AddItem(string name, int qty, decimal unitPrice) { _items.Add(new LineItem(name, qty, unitPrice)); return this; }

    public PaymentRequestBuilder WithCustomer(Action<CustomerBuilder> configure)
    { var b = new CustomerBuilder(); configure(b); _customer = b.Build(); return this; }

    public PaymentRequest Build()
    {
        if (string.IsNullOrWhiteSpace(_tenantId)) throw new PaymentValidationException("Tenant is required.");
        if (_amount is null || _amount.Amount <= 0) throw new PaymentValidationException("Amount must be positive.");
        if (_amount.Currency.Length != 3) throw new PaymentValidationException("Currency must be an ISO 4217 code.");
        if (string.IsNullOrWhiteSpace(_idempotencyKey)) throw new PaymentValidationException("Idempotency key is required.");
        if (_items.Count > 0 && _items.Sum(i => i.Qty * i.UnitPrice) != _amount.Amount)
            throw new PaymentValidationException("Items total must equal amount.");

        return new PaymentRequest(_tenantId, _orderId, _amount, _provider, _method, _threeDs, _customer,
            _items.AsReadOnly(), _returnUrl, _cancelUrl, _idempotencyKey, _metadata);
    }
}
