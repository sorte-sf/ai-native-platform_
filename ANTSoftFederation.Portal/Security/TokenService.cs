using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SorteFederation.Portal.Security;

public sealed class TokenService(IConfiguration configuration)
{
    public AuthResponse Create(User user, IEnumerable<UserRole> assignments)
    {
        var roles = assignments
            .Where(x => x.Role is not null)
            .Select(x => x.Role)
            .DistinctBy(x => x.Id)
            .ToArray();

        if (roles.Length == 0)
            throw new InvalidOperationException($"User '{user.Username}' has no role assignment.");

        var roleNames = roles.Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(GetRolePriority)
            .ThenBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissionNames = roles
            .SelectMany(x => x.RolePermissions ?? [])
            .Where(x => x.Permission is not null)
            .Select(x => x.Permission.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var jwt = configuration.GetSection("Auth:Jwt");
        var issuer = jwt["Issuer"] ?? configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("Auth:Jwt:Issuer is required.");
        var audience = jwt["Audience"] ?? configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("Auth:Jwt:Audience is required.");
        var key = jwt["Key"] ?? configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("Auth:Jwt:Key is required.");
        var expires = DateTimeOffset.UtcNow.AddHours(8);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username)
        };
        claims.AddRange(roleNames.Select(x => new Claim(ClaimTypes.Role, x)));

        if (user.CompanyId is not null)
        {
            claims.Add(new Claim("companyId", user.CompanyId.Value.ToString()));
            claims.Add(new Claim("tenantId", user.CompanyId.Value.ToString()));
        }

        claims.AddRange(permissionNames.Select(x => new Claim("permission", x)));

        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));

        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires,
            new CurrentUserResponse(user.Id.ToString(), user.CompanyId, user.Username, roleNames[0], permissionNames));
    }

    private static int GetRolePriority(string role) => role.ToLowerInvariant() switch
    {
        "SystemAdmin" or "systemAdmin" => 0,
        "admin" or "tenant admin" or "tenantadmin" => 1,
        "portal developer" => 2,
        "integrator" => 3,
        "portal user" => 4,
        "user" => 5,
        _ => 10
    };

}
