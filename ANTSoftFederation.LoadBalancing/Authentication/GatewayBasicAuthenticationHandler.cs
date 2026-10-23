
namespace SorteFederation.LoadBalancing.Authentication;

public sealed class GatewayBasicAuthenticationOptions : AuthenticationSchemeOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

#pragma warning disable CS0618 // The referenced ASP.NET authentication package exposes the ISystemClock constructor.
public sealed class GatewayBasicAuthenticationHandler(
    IOptionsMonitor<GatewayBasicAuthenticationOptions> options,
    ILoggerFactory logger,
    System.Text.Encodings.Web.UrlEncoder encoder,
    ISystemClock clock)
    : AuthenticationHandler<GatewayBasicAuthenticationOptions>(options, logger, encoder, clock)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var value) ||
            !AuthenticationHeaderValue.TryParse(value, out var header) ||
            !string.Equals(header.Scheme, "Basic", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(header.Parameter))
            return Task.FromResult(AuthenticateResult.Fail("A Basic Authorization header is required."));

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter));
            var separator = decoded.IndexOf(':');
            if (separator <= 0) return Task.FromResult(AuthenticateResult.Fail("Invalid Basic credentials."));

            var username = decoded[..separator];
            var password = decoded[(separator + 1)..];
            var valid = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(username), Encoding.UTF8.GetBytes(Options.Username)) &&
                CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(Options.Password));

            if (!valid) return Task.FromResult(AuthenticateResult.Fail("Invalid Basic credentials."));

            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, username), new Claim("auth_type", "basic")], Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
        catch (FormatException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Basic credentials."));
        }
    }
}
#pragma warning restore CS0618
