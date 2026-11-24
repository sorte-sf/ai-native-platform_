
namespace SorteFederation.PaymentGateway;

public sealed record CustomerDto(string? Email, string? FirstName, string? LastName, string? Country);
public sealed record ItemDto(string Name, int Qty, decimal UnitPrice);
public sealed record CreatePaymentDto(
    string OrderId, decimal Amount, string Currency, PaymentProviderType? Provider, PaymentMethod? Method,
    ThreeDSMode? ThreeDS, CustomerDto? Customer, List<ItemDto>? Items, string? ReturnUrl, string? CancelUrl);
public sealed record RefundDto(decimal? Amount);

public static class Endpoints
{ 
    private static string Tenant(HttpContext c) =>
        c.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? throw new PaymentValidationException("X-Tenant-Id header is required.");
    private static string IdemKey(HttpContext c) =>
        c.Request.Headers["Idempotency-Key"].FirstOrDefault() ?? throw new PaymentValidationException("Idempotency-Key header is required.");

    public static void MapPaymentEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/payments");

        g.MapGet("/methods", async (HttpContext ctx, ITenantPaymentConfigStore store, CancellationToken ct) =>
        {
            var cfg = await store.GetAsync(Tenant(ctx), ct);
            return cfg.Providers.Where(p => p.Value.Enabled).Select(p => p.Key.ToString());
        })
        .WithDescription("sorte-payment-gateway")
        .WithTags("get-payments");

        g.MapPost("/", async (CreatePaymentDto dto, HttpContext ctx, IPaymentService svc, CancellationToken ct) =>
        {
            var b = PaymentRequest.Create()
                .ForTenant(Tenant(ctx))
                .ForOrder(dto.OrderId)
                .WithAmount(dto.Amount, dto.Currency)
                .UseProvider(dto.Provider)                       // null → resolver decides
                .WithMethod(dto.Method ?? PaymentMethod.Card)
                .WithThreeDS(dto.ThreeDS ?? ThreeDSMode.Auto)
                .WithIdempotencyKey(IdemKey(ctx))
                .WithCustomer(c =>
                {
                    if (dto.Customer?.Email is { } e) c.Email(e);
                    c.Name(dto.Customer?.FirstName ?? "", dto.Customer?.LastName ?? "");
                    if (dto.Customer?.Country is { } cc) c.Country(cc);
                    c.Ip(ctx.Connection.RemoteIpAddress?.ToString());
                });
            if (dto.ReturnUrl is not null && dto.CancelUrl is not null) b.WithReturnUrls(dto.ReturnUrl, dto.CancelUrl);
            foreach (var i in dto.Items ?? []) b.AddItem(i.Name, i.Qty, i.UnitPrice);

            return Results.Ok(await svc.CreateAsync(b.Build(), ct));
        })
        .WithDescription("sorte-payment-gateway")
        .WithTags("post-payment");

        g.MapGet("/{id:guid}", async (Guid id, HttpContext ctx, IPaymentService svc, CancellationToken ct) =>
        {
            var p = await svc.GetAsync(id, Tenant(ctx), ct);
            return Results.Ok(new { paymentId = p.Id, status = p.Status, provider = p.Provider, amount = p.Amount, refundedTotal = p.RefundedTotal });
        })
        .WithDescription("sorte-payment-gateway")
        .WithTags("get-payment-byid");


        g.MapPost("/{id:guid}/confirm", async (Guid id, Dictionary<string, string>? data, HttpContext ctx, IPaymentService svc, CancellationToken ct) =>
            Results.Ok(await svc.ConfirmAsync(id, Tenant(ctx), data ?? [], ct))
         )
        .WithDescription("sorte-payment-gateway")
        .WithTags("confirm-payment-byid");


        g.MapPost("/{id:guid}/refunds", async (Guid id, RefundDto dto, HttpContext ctx, IPaymentService svc, CancellationToken ct) =>
            Results.Ok(await svc.RefundAsync(id, Tenant(ctx), dto.Amount, IdemKey(ctx), ct))
         )
        .WithDescription("sorte-payment-gateway")
        .WithTags("payment-refunds");


    }

    public static void MapWebhookEndpoints(this WebApplication app) =>
        app.MapPost("/webhooks/{provider}/{tenantKey}", async (string provider, string tenantKey, HttpRequest req, IPaymentService svc, CancellationToken ct) =>
        {
            if (!Enum.TryParse<PaymentProviderType>(provider, true, out var type)) return Results.NotFound();

            using var reader = new StreamReader(req.Body);
            var payload = await reader.ReadToEndAsync(ct); 
            var headers = req.Headers.ToDictionary(h => h.Key, h => h.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            await svc.HandleWebhookAsync(type, tenantKey, new WebhookContext(payload, headers), ct);
            return Results.Ok(); 
        })
       .WithDescription("sorte-payment-gateway")
       .WithTags("webhooks");

}

public sealed class PaymentExceptionMiddleware(RequestDelegate next, ILogger<PaymentExceptionMiddleware> log)
{
    public async Task Invoke(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (PaymentException ex)
        {
            var status = ex switch
            {
                PaymentValidationException => StatusCodes.Status400BadRequest,
                WebhookSignatureException => StatusCodes.Status400BadRequest,
                PaymentNotFoundException => StatusCodes.Status404NotFound,
                ProviderNotEnabledException or PaymentConfigurationException => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status502BadGateway
            };
            if (status >= 500) log.LogError(ex, "Payment failure");
            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.GetType().Name.Replace("Exception", ""), message = ex.Message });
        }
    }
}

