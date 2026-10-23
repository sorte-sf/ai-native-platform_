
using SorteFederation.Portal.Services;
using SorteFederation.Shared;

namespace SorteFederation.Portal.EndPoints.Auths;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
            LoginRequest request,
            ANTSoftPortalDbContext db,
            TokenService tokens,
            ISecurityService security,
            HttpContext httpContext,
            IWebHostEnvironment environment,
            CancellationToken cancellationToken) =>
        {
            var user = await db.Users.Include(x => x.UserRoles)
                                     .ThenInclude(x => x.Role)
                                     .ThenInclude(x => x.RolePermissions)
                                     .ThenInclude(x => x.Permission)
                               .SingleOrDefaultAsync(x => x.Username == request.UserName.Trim() || x.Email == request.UserName.Trim(), cancellationToken);

            if (user is null || user.IsLockedOut == true ||
                user.Password is null || user.Salt is null ||
                !security.VerifyPasswordHash(request.Password, user.Password, user.Salt))
                return Results.Unauthorized();

            if (!user.UserRoles.Any(x => x.Role is not null)) return Results.Problem("The user account has no role assignment.", statusCode: 500);

            var auth = tokens.Create(user, user.UserRoles);
            WriteSsoCookie(httpContext, auth.AccessToken, auth.ExpiresAt, environment);
            return Results.Ok(new
            {
                accessToken = auth.AccessToken,
                expiresAt = auth.ExpiresAt,
                user = auth.User,
                mustChangePassword = user.ChangePasswordOnFirstLogin == true
            });
        });

        app.MapGet("/auth/me", (ClaimsPrincipal user) => user.Identity?.IsAuthenticated == true
            ? Results.Ok(new CurrentUserResponse(user.FindFirstValue(ClaimTypes.NameIdentifier)!, null, user.Identity.Name!, user.FindFirstValue(ClaimTypes.Role)!, user.FindAll("permission").Select(x => x.Value).Order().ToArray()))
            : Results.Unauthorized()).RequireAuthorization();

        app.MapPost("/auth/change-password", async (ChangePasswordRequest request, ClaimsPrincipal principal, ANTSoftPortalDbContext db, ISecurityService security, CancellationToken cancellationToken) =>
        {
            if (request is null || !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ||
                string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6 ||
                request.NewPassword != request.ConfirmPassword)
                return Results.BadRequest("Invalid password change request.");

            var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (user is null) return Results.Unauthorized();
            user.Salt = security.GetpasswordSalt();
            user.Password = security.GetpasswordHash(user.Salt, request.NewPassword);
            user.ChangePasswordOnFirstLogin = false;

            await db.SaveChangesAsync(cancellationToken);

            return Results.NoContent();

        }).RequireAuthorization();

        app.MapPost("/auth/verify-otp", async (UserOtpVerificationDto request, TokenService tokens, ANTSoftPortalDbContext db,
            HttpContext httpContext, IWebHostEnvironment environment, CancellationToken cancellationToken) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.UserName || u.Email == request.UserName, cancellationToken);

            if (user is null)
                return Results.BadRequest("User_not-found.");

            var isValidOtp = OtpService.ValidateOtpForUser(request.OtpCode, user.Username);

            if (!isValidOtp)
                return Results.BadRequest("OTP is invalid or has expired.");

            var auth = tokens.Create(user, user.UserRoles);
            WriteSsoCookie(httpContext, auth.AccessToken, auth.ExpiresAt, environment);

            return Results.Ok(new
            {
                accessToken = auth.AccessToken,
                expiresAt = auth.ExpiresAt,
                user = auth.User,
                mustChangePassword = user.ChangePasswordOnFirstLogin == true
            });

        }).RequireAuthorization();

        app.MapPost("/auth/refresh", async (
            ClaimsPrincipal principal,
            ANTSoftPortalDbContext db,
            TokenService tokens,
            HttpContext httpContext,
            IWebHostEnvironment environment,
            CancellationToken cancellationToken) =>
        {
            var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Results.Unauthorized();
            }

            var user = await db.Users
                .Include(x => x.UserRoles).ThenInclude(x => x.Role).ThenInclude(x => x.RolePermissions).ThenInclude(x => x.Permission)
                .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null || user.IsLockedOut == true || user.UserRoles is null)
            {
                return Results.Unauthorized();
            }

            var auth = tokens.Create(user, user.UserRoles);
            WriteSsoCookie(httpContext, auth.AccessToken, auth.ExpiresAt, environment);
            return Results.Ok(new
            {
                auth.AccessToken,
                auth.ExpiresAt,
                auth.User,
                mustChangePassword = user.ChangePasswordOnFirstLogin == true
            });

        }).RequireAuthorization();

        app.MapPost("/auth/logout", (HttpContext httpContext, IWebHostEnvironment environment) =>
        {
            httpContext.Response.Cookies.Delete(SsoTokenCookie.Name, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = !environment.IsDevelopment() || httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });
            return Results.NoContent();
        }).AllowAnonymous();

        return app;

    }

    private sealed record ChangePasswordRequest(string NewPassword, string ConfirmPassword);

    private static void WriteSsoCookie(HttpContext context, string token, DateTimeOffset expiresAt, IWebHostEnvironment environment)
    {
        context.Response.Cookies.Append(SsoTokenCookie.Name, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment() || context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = expiresAt
        });
    }
}
