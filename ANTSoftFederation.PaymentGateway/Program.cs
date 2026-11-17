
using Figgle.Fonts;
using SorteFederation.PaymentGateway;
using SorteFederation.PaymentGateway.Abstractions;
using SorteFederation.PaymentGateway.Interfaces;
using SorteFederation.PaymentGateway.Providers.Iyzico;
using SorteFederation.PaymentGateway.Providers.PayPal;
using SorteFederation.PaymentGateway.Providers.Stripe;
using SorteFederation.Shared;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PaymentTenantsOptions>(builder.Configuration.GetSection("Payments"));
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

builder.Services.ConfigureCorsPolicy();

builder.Services.AddPaymentPlatform(p => p
    .AddStripe()
    .AddIyzico()
    .AddPayPal()
    .UseTenantConfigStore<ConfigurationTenantConfigStore>()
    .WithDefaultProvider(PaymentProviderType.Stripe)
    .WithRouting(r => r
        .When(c => c.Currency == "TRY").Use(PaymentProviderType.Iyzico)
        .When(c => c.Method == PaymentMethod.Wallet).Use(PaymentProviderType.PayPal))
    .WithFallback(PaymentProviderType.Stripe, PaymentProviderType.PayPal));

var app = builder.Build();

app.UseCors("SortePolicy");

app.UseMiddleware<PaymentExceptionMiddleware>();
app.MapPaymentEndpoints();
app.MapWebhookEndpoints();

var figgleText = FiggleFonts.Standard.Render(builder.Configuration.GetSection("app:name").Value ?? "Sorte-Gateway");
app.MapGet("/", ctx => ctx.Response.WriteAsync(figgleText));

app.Run();


public partial class Program; 
