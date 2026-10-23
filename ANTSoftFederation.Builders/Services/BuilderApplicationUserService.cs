using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data.Domain;
using SorteFederation.Builders.Data.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace SorteFederation.Builders.Services;

public sealed class BuilderApplicationUserService(
    BuilderDbContext db,
    IPasswordHasher<BuilderApplicationUser> hasher,
    IConfiguration configuration)
{
    public async Task<BuilderAppUserLoginResponse?> Login(BuilderAppUserLoginRequest request, CancellationToken token)
    {
        var email = NormalizeEmail(request.Email);
        var app = await db.Applications.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId &&
                x.Status == DefinitionStatus.Published && x.RecordState == RecordState.Active, token);
        if (app is null) return null;

        var user = await db.ApplicationUsers.IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.TenantId == request.TenantId && x.NormalizedEmail == email && x.RecordState == RecordState.Active, token);
        if (user is null || !user.IsActive || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return null;

        var memberships = await db.ApplicationMemberships.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && x.UserId == user.Id && x.IsActive && x.RecordState == RecordState.Active)
            .ToListAsync(token);
        var groupIds = await db.UserGroupMemberships.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && x.UserId == user.Id && x.IsActive && x.RecordState == RecordState.Active &&
                x.Group.IsActive && x.Group.RecordState == RecordState.Active)
            .Select(x => x.GroupId).ToListAsync(token);
        List<string> appRoles = groupIds.Count == 0 ? [] : await db.GroupRoleAssignments.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && x.ApplicationId == app.Id && groupIds.Contains(x.GroupId) && x.IsActive &&
                x.RecordState == RecordState.Active && x.Role.IsActive && x.Role.RecordState == RecordState.Active)
            .Select(x => x.Role.Key).Distinct().ToListAsync(token);
        if (!memberships.Any(x => x.ApplicationId == app.Id) && appRoles.Count == 0) return null;

        var jwtSettings = configuration.GetSection("Auth:BuilderUsers");
        var signingKey = configuration["Auth:Jwt:Key"] ?? throw new InvalidOperationException("Auth:Jwt:Key is required to issue Builder application-user tokens.");
        if (Encoding.UTF8.GetByteCount(signingKey) < 32) throw new InvalidOperationException("Auth:Jwt:Key must contain at least 32 bytes.");
        var issuer = jwtSettings["Issuer"] ?? "sorte.builder.users";
        var audience = jwtSettings["Audience"] ?? "antsoft.builder.apps";
        var expires = DateTimeOffset.UtcNow.AddMinutes(45);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new("tenant_id", request.TenantId.ToString()),
            new("builder_user", "true"),
            new("security_version", user.SecurityVersion.ToString()),
            new("builder_application_id", app.Id.ToString())
        };
        claims.AddRange(memberships.Where(x => x.ApplicationId == app.Id).Select(x => new Claim("builder_app_membership", $"{x.ApplicationId:N}:{x.Role}")));
        claims.AddRange(memberships.Where(x => x.ApplicationId == app.Id).Select(x => new Claim("builder_app_role", x.Role)));
        claims.AddRange(groupIds.Select(x => new Claim("builder_group_id", x.ToString())));
        claims.AddRange(appRoles.Select(x => new Claim("builder_app_role", x)));
        var jwt = new JwtSecurityToken(issuer, audience, claims, expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256));
        var tokenText = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new BuilderAppUserLoginResponse(tokenText, expires,
            new { id = user.Id, user.Email, user.DisplayName, tenantId = user.TenantId }, user.MustChangePassword);
    }

    public async Task<object> CreateOrAddMembership(CreateBuilderAppUserRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Length > 200)
            throw new ArgumentException("A valid email and display name are required.");
        if (request.Password.Length < 6) throw new ArgumentException("Initial passwords must contain at least 6 characters.");
        if (string.IsNullOrWhiteSpace(request.Role) || request.Role.Length > 80) throw new ArgumentException("A valid application role is required.");
        var app = await db.Applications.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token)
            ?? throw new KeyNotFoundException("Application was not found for this tenant.");
        var email = request.Email.Trim();
        var normalized = NormalizeEmail(email);
        var user = await db.ApplicationUsers.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == request.TenantId && x.NormalizedEmail == normalized, token);
        if (user is null)
        {
            await TenantSubscriptionQuota.EnsureCanCreateUser(db, request.TenantId, token);
            user = new BuilderApplicationUser
            {
                TenantId = request.TenantId,
                Email = email,
                NormalizedEmail = normalized,
                DisplayName = request.DisplayName.Trim(),
                PasswordHash = string.Empty,
                MustChangePassword = true
            };
            user.PasswordHash = hasher.HashPassword(user, request.Password);
            db.ApplicationUsers.Add(user);
        }
        else if (user.RecordState != RecordState.Active)
            throw new InvalidOperationException("This account has been deleted and cannot be reactivated by adding a membership.");

        if (await db.ApplicationMemberships.IgnoreQueryFilters().AnyAsync(x => x.TenantId == request.TenantId && x.ApplicationId == app.Id && x.UserId == user.Id, token))
            throw new InvalidOperationException("This user already has a membership in the application.");

        db.ApplicationMemberships.Add(new BuilderApplicationMembership
        {
            TenantId = request.TenantId, ApplicationId = app.Id, UserId = user.Id, Role = request.Role.Trim()
        });
        user.SecurityVersion++;
        await db.SaveChangesAsync(token);
        return new { user.Id, user.Email, user.DisplayName, user.IsActive, applicationId = app.Id, role = request.Role.Trim() };
    }

    public async Task<IReadOnlyList<object>> Users(Guid tenantId, CancellationToken token) =>
        await db.ApplicationUsers.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RecordState == RecordState.Active)
            .OrderBy(x => x.DisplayName)
            .Select(x => (object)new { x.Id, x.Email, x.DisplayName, x.IsActive, x.MustChangePassword, x.CreatedAt,
                applicationCount = x.Memberships.Count(m => m.TenantId == tenantId && m.IsActive && m.RecordState == RecordState.Active) })
            .ToListAsync(token);

    public async Task<IReadOnlyList<object>> PublishedApplications(Guid tenantId, CancellationToken token) =>
        await db.Applications.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == DefinitionStatus.Published && x.RecordState == RecordState.Active)
            .OrderBy(x => x.Name)
            .Select(x => (object)new { x.Id, x.Name, x.Key, x.PublishedVersion })
            .ToListAsync(token);

    public async Task<IReadOnlyList<object>> ApplicationMembers(Guid tenantId, Guid applicationId, CancellationToken token)
    {
        var appExists = await db.Applications.IgnoreQueryFilters().AnyAsync(x => x.Id == applicationId && x.TenantId == tenantId &&
            x.Status == DefinitionStatus.Published && x.RecordState == RecordState.Active, token);
        if (!appExists) return [];
        return await db.ApplicationMemberships.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.IsActive && x.RecordState == RecordState.Active &&
                x.User.IsActive && x.User.RecordState == RecordState.Active)
            .OrderBy(x => x.User.DisplayName)
            .Select(x => (object)new { userId = x.UserId, x.User.Email, x.User.DisplayName, x.Role, x.CreatedAt })
            .ToListAsync(token);
    }

    public async Task<object> CreateTenantUser(Guid tenantId, CreateTenantBuilderUserRequest request, CancellationToken token)
    {
        var email = (request.Email ?? string.Empty).Trim();
        var displayName = (request.DisplayName ?? string.Empty).Trim();
        if (email.Length is < 3 or > 320 || !email.Contains('@') || displayName.Length is < 1 or > 200)
            throw new ArgumentException("Enter a valid email address and a display name.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ArgumentException("Initial passwords must contain at least 6 characters.");
        var normalized = NormalizeEmail(email);
        if (await db.ApplicationUsers.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.NormalizedEmail == normalized, token))
            throw new InvalidOperationException("A user with this email already exists in the tenant.");
        await TenantSubscriptionQuota.EnsureCanCreateUser(db, tenantId, token);
        var user = new BuilderApplicationUser
        {
            TenantId = tenantId,
            Email = email,
            NormalizedEmail = normalized,
            DisplayName = displayName,
            PasswordHash = string.Empty,
            MustChangePassword = true
        };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.ApplicationUsers.Add(user);
        await db.SaveChangesAsync(token);
        return new { user.Id, user.Email, user.DisplayName, user.IsActive, user.MustChangePassword, user.CreatedAt, applicationCount = 0 };
    }

    public async Task<object> AssignMembership(Guid tenantId, Guid applicationId, AssignBuilderAppMembershipRequest request, CancellationToken token)
    {
        if (request.UserId == Guid.Empty) throw new ArgumentException("Select a tenant user.");
        var role = (request.Role ?? string.Empty).Trim();
        if (role.Length is < 1 or > 80) throw new ArgumentException("Enter an application role.");
        var app = await db.Applications.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId &&
            x.Status == DefinitionStatus.Published && x.RecordState == RecordState.Active, token)
            ?? throw new KeyNotFoundException("Published application was not found.");
        var user = await db.ApplicationUsers.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == request.UserId && x.TenantId == tenantId &&
            x.IsActive && x.RecordState == RecordState.Active, token)
            ?? throw new KeyNotFoundException("Active tenant user was not found.");
        var membership = await db.ApplicationMemberships.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId &&
            x.ApplicationId == app.Id && x.UserId == user.Id, token);
        if (membership is null)
        {
            membership = new BuilderApplicationMembership { TenantId = tenantId, ApplicationId = app.Id, UserId = user.Id, Role = role };
            db.ApplicationMemberships.Add(membership);
        }
        else
        {
            membership.Role = role;
            membership.IsActive = true;
            membership.RecordState = RecordState.Active;
            membership.UpdatedAt = DateTimeOffset.UtcNow;
        }
        user.SecurityVersion++;
        await db.SaveChangesAsync(token);
        return new { userId = user.Id, user.Email, user.DisplayName, applicationId = app.Id, applicationName = app.Name, role, isActive = true };
    }

    public async Task<bool> RemoveMembership(Guid tenantId, Guid applicationId, Guid userId, CancellationToken token)
    {
        var membership = await db.ApplicationMemberships.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId &&
            x.ApplicationId == applicationId && x.UserId == userId && x.IsActive && x.RecordState == RecordState.Active, token);
        if (membership is null) return false;
        membership.IsActive = false;
        membership.UpdatedAt = DateTimeOffset.UtcNow;
        var user = await db.ApplicationUsers.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId, token);
        if (user is not null) user.SecurityVersion++;
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> ChangePassword(Guid tenantId, string userId, ChangeBuilderAppUserPasswordRequest request, CancellationToken token)
    {
        if (!Guid.TryParse(userId, out var id) || string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 12 || request.NewPassword != request.ConfirmPassword)
            throw new ArgumentException("Enter the current password and a matching new password of at least 12 characters.");
        var user = await db.ApplicationUsers.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.IsActive, token);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed) return false;
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        user.MustChangePassword = false;
        user.SecurityVersion++;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);
        return true;
    }

    private static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToUpperInvariant();
}
