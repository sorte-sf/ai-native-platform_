
//using Microsoft.Extensions.Options;
//using System.Text.Encodings.Web;

//namespace ANTSoftFederation.IdentityServer.Middlewares;

//public class BasicAuthenticationOptions : AuthenticationSchemeOptions
//{
//}

//public class CustomAuthenticationMiddleware : AuthenticationHandler<BasicAuthenticationOptions>
//{
//    private readonly ButterflyIdentityDbContext _context;
//    private readonly JwtSettings _jwtSettings;

//    public CustomAuthenticationMiddleware(
//        IOptionsMonitor<BasicAuthenticationOptions> options,
//        ILoggerFactory logger,
//        UrlEncoder encoder,
//        ButterflyIdentityDbContext context,
//        IOptions<JwtSettings> jwtSettings)
//        : base(options, logger, encoder)
//    {
//        this._context = context;
//        this._jwtSettings = jwtSettings.Value;
//    }

//    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
//    { 
//        if (!this.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
//        {
//            return AuthenticateResult.Fail("Missing Authorization header");
//        }
         
//        if (string.IsNullOrWhiteSpace(authorizationHeader))
//        {
//            return AuthenticateResult.NoResult();
//        } 

//        const string bearerPrefix = "bearer ";
//        if (!authorizationHeader.ToString().StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
//        {
//            return AuthenticateResult.Fail("Invalid authorization scheme");
//        }
         
//        var token = authorizationHeader.ToString().Substring(bearerPrefix.Length).Trim();
         
//        if (string.IsNullOrWhiteSpace(token))
//        {
//            return AuthenticateResult.Fail("Missing token");
//        }

//        try
//        {
//            return await this.ValidateTokenAsync(token);
//        }
//        catch (SecurityTokenException ex)
//        {
//            return AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
//        }
//        catch (Exception ex)
//        { 
//            Logger.LogError(ex, "Unexpected error during token validation");
//            return AuthenticateResult.Fail("Authentication failed");
//        }
//    }

//    private async Task<AuthenticateResult> ValidateTokenAsync(string token)
//    { 
//        var tokenHandler = new JwtSecurityTokenHandler();
         
//        var tokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = _jwtSettings.Issuer,
//            ValidAudience = _jwtSettings.Audience,
//            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
//            ClockSkew = TimeSpan.Zero // Remove default 5-minute clock skew
//        };

//        try
//        { 
//            var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
             
//            if (!(validatedToken is JwtSecurityToken jwtSecurityToken))
//            {
//                return AuthenticateResult.Fail("Invalid token format");
//            }

//            var userIdClaim = claimsPrincipal.FindFirst("unique_name") ??  claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            
//            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
//            {
//                return AuthenticateResult.Fail("Invalid user identifier in token");
//            }

//            var userToken = await this._context.UserSessions.FirstOrDefaultAsync(t => t.Token == token);

//            if (userToken == null)
//            {
//                return AuthenticateResult.Fail("Invalid token");
//            }

//            if (userToken.RevokedAt != DateTimeOffset.MinValue)
//            {
//                return AuthenticateResult.Fail("Token revoked");
//            }
             
//            var now = DateTime.UtcNow;
//            if (userToken.ExpiredTime <= now)
//            {
//                return AuthenticateResult.Fail("Token expired");
//            }
             
//            if (userToken.UserId != userId)
//            {
//                return AuthenticateResult.Fail("Token user mismatch");
//            }
             
//            var emailClaim = claimsPrincipal.FindFirst("email");
//            var roleClaim = claimsPrincipal.FindFirst("roles");
             
//            var claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
//                new Claim(ClaimTypes.Name, userId.ToString())
//            };
             
//            if (emailClaim != null)
//            {
//                claims.Add(new Claim(ClaimTypes.Email, emailClaim.Value));
//            }

//            if (roleClaim != null)
//            {
//                claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
//            }
             
//            var identity = new ClaimsIdentity(claims, Scheme.Name);
//            var principal = new System.Security.Principal.GenericPrincipal(identity, null);
//            var ticket = new AuthenticationTicket(principal, Scheme.Name);

//            return AuthenticateResult.Success(ticket);
//        }
//        catch (SecurityTokenExpiredException)
//        {
//            return AuthenticateResult.Fail("Token expired");
//        }
//        catch (SecurityTokenInvalidSignatureException)
//        {
//            return AuthenticateResult.Fail("Invalid token signature");
//        }
//        catch (SecurityTokenException)
//        {
//            throw;
//        }
//    }
//}
