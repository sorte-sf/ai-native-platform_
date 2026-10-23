 
namespace ANTSoftFederation.IdentityServer.Services.Auths;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    RefreshToken GenerateRefreshToken(Guid userId);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("username", user.Username),
            new("displayName ", user.DisplayName ),
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            //Expires = DateTime.UtcNow.AddSeconds(project.AccessTokenLifespan),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId)
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            ExpiryDate = DateTime.UtcNow.AddDays(7), // 7 days
            CreatedAt = DateTime.UtcNow
        };
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false // Don't validate lifetime for refresh token scenario
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public static async Task<string> GenerateAsync(
      User user,
      string refreshToken,
      ANTSoftIdentityDbContext db,
      IConfiguration configuration)
    {
        try
        {
            var subject = configuration.GetSection("Authentication:jwt:subject").Value;
            var secrets = configuration.GetSection("Authentication:jwt:secretkey").Value;
            var issuer = configuration.GetSection("Authentication:jwt:issuer").Value;
            var audience = configuration.GetSection("Authentication:jwt:audience").Value;

            var tokenExpirationMinutes = configuration.GetValue<int>("Authentication:AccessTokenExpirationMinutes");
            if (tokenExpirationMinutes <= 0)
            {
                tokenExpirationMinutes = 4320;
            }

            var nowUtc = DateTime.UtcNow;

            var _iat = DateTimeOffset.UtcNow.ToLocalTime().AddMinutes(tokenExpirationMinutes);
            var tokenExpires = nowUtc.AddMinutes(tokenExpirationMinutes);

            var userRoles = await db.UserRoles.Where(x => x.UserId == user.Id).Select(d => d.Role.Name).ToArrayAsync();

            var userProjects = await db.UserProjects.Where(x => x.UserId == user.Id)
                                       .Select(d => new { Id = d.ProjectId, Name = d.Project.Name })
                                       .ToListAsync();

            var projectsJson = userProjects.Any() ? System.Text.Json.JsonSerializer.Serialize(userProjects.Select(p => new
            {
                projectId = p.Id,
                projectName = p.Name
            }).ToArray())
           : string.Empty;

            var claims = new[]
            {
               new Claim(JwtRegisteredClaimNames.Sub, subject),
               new Claim(JwtRegisteredClaimNames.UniqueName, user.Id.ToString()),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
               new Claim(JwtRegisteredClaimNames.Iat, _iat.ToUnixTimeSeconds().ToString()),
               new Claim("userId", user.Id.ToString()),
               new Claim("displayName", user.DisplayName ?? string.Empty),
               new Claim("userName", user.Username ),
               new Claim("refreshToken", refreshToken),
               new Claim("roles", userRoles.Length > 0 ? string.Join(",", userRoles) : string.Empty),
               new Claim("UserProjects", projectsJson),
               new Claim("email", user.Email  ?? string.Empty)
         };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                notBefore: nowUtc,
                expires: tokenExpires,
                signingCredentials: signIn);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

}
