
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PaymentTenantsOptions>(builder.Configuration.GetSection("Payments"));
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

builder.Services.ConfigureCorsPolicy();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, _) =>
    {
        document.Info = new()
        {
            Title = "Sorte Payment Documentations",
            Version = "v1",
            Description = """
                Modern API for managing Payment API.
                Supports JSON and XML responses.
                Rate limited to 1000 requests per hour.
                """,
            Contact = new()
            {
                Name = "API Support"
            }
        };
        return Task.CompletedTask;
    });
});

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

app.MapOpenApi();

app.MapScalarApiReference(o =>
{
    o.DarkMode = true;
    o.WithTitle("Sorte PAyment API Documentation")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

    o.Authentication = new ScalarAuthenticationOptions
    {

    };
});

app.Run();


public partial class Program; 
