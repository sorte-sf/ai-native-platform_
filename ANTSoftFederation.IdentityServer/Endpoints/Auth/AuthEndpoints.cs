namespace ANTSoftFederation.IdentityServer.Bootstraping;

public class AuthEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("auth");

        group.MapPost("/login", async (LoginRequestDto request, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);
            if (!result.Success && result.MessageCode == "invalid_user_credentials") return Results.BadRequest(result);
            if (!result.Success && result.MessageCode == "2fa_required") return Results.Ok(result);
            if (!result.Success) return Results.Forbid();
            return Results.Ok(result);
        })
        .WithName("system-user-login");

        group.MapPost("/webhook", async (LoginRequestDto request, string redirectUrl, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);
            if (result.Success && !string.IsNullOrEmpty(result.AccessToken))
            {
                var url = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(redirectUrl, "token", result.AccessToken);
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    url = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(url, "refreshToken", result.RefreshToken);
                }
                return Results.Redirect(url, permanent: false, preserveMethod: false);
            }
            return Results.BadRequest(result);
        })
        .WithName("webhook-sso-login");

        group.MapPost("/verify-otp", async (UserOtpVerificationDto model, IAuthService authService) =>
        {
            var result = await authService.VerifyOtpAsync(model);
            if (result.Success) return Results.Ok(result);
            if (result.MessageCode == "user_not_found") return Results.NotFound(result);
            return Results.BadRequest(result);
        })
        .WithName("verify-otp");

        group.MapPost("/logout", async (LogoutModel request, IAuthService authService) =>
        {
            var success = await authService.LogoutAsync(request);
            if (success)
            {
                return Results.Ok(new { Success = true, MessageCode = "user_logout_successfully", Message = "User logged out." });
            }
            return Results.BadRequest(new { Success = false, MessageCode = "logout_failed", Message = "User not found or logout failed." });
        })
        .WithName("user-logout");

        group.MapPost("/refresh-token", async ([FromBody] RefreshTokenModel request, IAuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(request);
            if (result.Success) return Results.Ok(result);
            return Results.BadRequest(result);
        })
        .WithName("refresh-token");

        group.MapPost("/auth/generate", (string username, TwoFactorService twoFactorService) =>
        {
            var settings = twoFactorService.Generate2FASecret(username);
            return Results.Ok(settings);
        })
        .WithName("generate-2fa");

        group.MapPost("/auth/validate", (string secretKey, string token, TwoFactorService twoFactorService) =>
        {
            var isValid = twoFactorService.Validate2FAToken(secretKey, token);
            return Results.Ok(isValid);
        })
        .WithName("validate-2fa");

        group.MapPost("/auth/token/generate", (int expirationInMinutes, TwoFactorService twoFactorService) =>
        {
            var token = twoFactorService.GenerateOneTimeToken(expirationInMinutes);
            return Results.Ok(new { Token = token, ExpirationInMinutes = expirationInMinutes });
        })
        .WithName("token-generate");

        group.MapPost("/auth/token/validate", (string token, TwoFactorService twoFactorService) =>
        {
            var isValid = twoFactorService.ValidateOneTimeToken(token);
            return Results.Ok(isValid);
        })
        .WithName("token-validate");
    }
}
