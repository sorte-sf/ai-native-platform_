
namespace ANTSoftFederation.IdentityServer.Middlewares;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ANTSoftIdentityDbContext _dbContext;
    private readonly ISecurityService _securityService;

    [Obsolete("ISystemClock is obsolete, use TimeProvider on new OSs, but sticking to standard implementation for .NET 8/10 compatibility")]
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        ANTSoftIdentityDbContext dbContext,
        ISecurityService securityService)
        : base(options, logger, encoder, clock)
    {
        _dbContext = dbContext;
        _securityService = securityService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        User user = null;

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (authHeader.Scheme.ToLower() != "basic")
                return AuthenticateResult.Fail("Invalid Authorization Scheme");

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            var username = credentials[0];
            var password = credentials[1];

            user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Username == username);

            if (user == null || user.IsLockedOut == true)
                return AuthenticateResult.Fail("Invalid Username or Password");

            var succeeded = _securityService.VerifyPasswordHash(password, user.Password, user.Salt);
            if (!succeeded)
                return AuthenticateResult.Fail("Invalid Username or Password");
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header Structure");
        }

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("status", "Authenticated via Basic")
        };
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
