using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace SorteFederation.Builders.Services;

public sealed class BuilderWorkspaceAuthService(
    BuilderDbContext db,
    IPasswordHasher<BuilderApplicationUser> hasher,
    IConfiguration configuration)
{
    public async Task<object?> Login(BuilderWorkspaceLoginRequest request, CancellationToken token)
    {
        var email = (request.Email ?? string.Empty).Trim();
        if (email.Length is < 3 or > 320 || string.IsNullOrWhiteSpace(request.Password)) return null;
        var normalized = email.ToUpperInvariant();
        var candidates = await db.WorkspaceAccess.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.IsActive && x.RecordState == RecordState.Active && x.User.IsActive &&
                x.User.RecordState == RecordState.Active && x.User.NormalizedEmail == normalized)
            .Select(x => new { Access = x, User = x.User })
            .ToListAsync(token);
        var match = candidates.FirstOrDefault(x => hasher.VerifyHashedPassword(x.User, x.User.PasswordHash, request.Password) != PasswordVerificationResult.Failed);
        if (match is null) return null;

        return CreateSession(match.Access, match.User);
    }

    public async Task<object?> Refresh(ClaimsPrincipal principal, CancellationToken token)
    {
        if (!Guid.TryParse(principal.FindFirstValue("builder_workspace_access_id"), out var accessId) ||
            !Guid.TryParse(principal.FindFirstValue("tenant_id"), out var tenantId) ||
            !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"), out var userId)) return null;

        var access = await db.WorkspaceAccess.IgnoreQueryFilters().Include(x => x.User).SingleOrDefaultAsync(x =>
            x.Id == accessId && x.TenantId == tenantId && x.UserId == userId && x.IsActive &&
            x.RecordState == RecordState.Active && x.User.IsActive && x.User.RecordState == RecordState.Active, token);
        return access is null ? null : CreateSession(access, access.User);
    }

    private object CreateSession(BuilderWorkspaceAccess access, BuilderApplicationUser user)
    {
        var signingKey = configuration["Auth:Jwt:Key"] ?? throw new InvalidOperationException("Auth:Jwt:Key is required to issue Builder workspace tokens.");
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim("builder_workspace_user", "true"),
            new Claim("builder_workspace_access_id", access.Id.ToString()),
            new Claim("builder_workspace_security_version", access.SecurityVersion.ToString()),
            new Claim("builder_workspace_role", access.Role)
        };
        claims.Add(new Claim(ClaimTypes.Role, access.Role is "BuilderAdmin" or "BuilderEditor" ? BuilderRoleNames.Developer : "BuilderViewer"));

        var expires = DateTimeOffset.UtcNow.AddMinutes(45);
        var issuer = configuration["Auth:BuilderWorkspace:Issuer"] ?? "sorte.builder.workspace";
        var audience = configuration["Auth:BuilderWorkspace:Audience"] ?? "antsoft.builder.workspace";
        var jwt = new JwtSecurityToken(issuer, audience, claims, expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256));
        return new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(jwt), expiresAt = expires,
            userId = user.Id.ToString(), email = user.Email, displayName = user.DisplayName,
            userRoles = access.Role is "BuilderAdmin" or "BuilderEditor"
                ? new[] { access.Role, BuilderRoleNames.Developer } : new[] { access.Role },
            permissions = Array.Empty<string>(),
            user = new { id = user.Id.ToString(), companyId = user.TenantId.ToString(), userName = user.DisplayName, role = access.Role, permissions = Array.Empty<string>() },
            mustChangePassword = user.MustChangePassword, authMode = "builderWorkspace"
        };
    }

    public async Task<bool> ChangePassword(ClaimsPrincipal principal, ChangeBuilderAppUserPasswordRequest request, CancellationToken token)
    {
        if (!Guid.TryParse(principal.FindFirstValue("tenant_id"), out var tenantId) ||
            !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"), out var userId)) return false;
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8 || request.NewPassword.Length > 100 ||
            request.NewPassword != request.ConfirmPassword) throw new ArgumentException("New password must be at least 8 characters and match the confirmation.");
        var user = await db.ApplicationUsers.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId &&
            x.IsActive && x.RecordState == RecordState.Active, token);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed) return false;
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        user.MustChangePassword = false;
        user.SecurityVersion++;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);
        return true;
    }
}
