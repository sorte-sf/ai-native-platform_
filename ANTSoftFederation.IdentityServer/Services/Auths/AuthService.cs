using static Google.Protobuf.Reflection.ExtensionRangeOptions.Types;
using static StackExchange.Redis.Role;

namespace ANTSoftFederation.IdentityServer.Services.Auths;

public class AuthService : IAuthService
{
    private readonly ANTSoftIdentityDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ISecurityService _securityService;
    private readonly IMailKitService _mailKitService;

    public AuthService(
        ANTSoftIdentityDbContext dbContext,
        IConfiguration configuration,
        ISecurityService securityService,
        IMailKitService mailKitService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _securityService = securityService;
        _mailKitService = mailKitService;
    } 

    public async Task<LoginRespondDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return new LoginRespondDto { Success = false, MessageCode = "invalid_user_credentials" };

        var userData = await _dbContext.Users
            .Where(x => x.Username == request.Username || x.TelePhone == request.Username)
            .SingleOrDefaultAsync();

        if (userData == null)
            return new LoginRespondDto { Success = false, MessageCode = "invalid_user_credentials" };

        if (userData.IsLockedOut == true)
            return new LoginRespondDto { Success = false, MessageCode = "account_locked" };

        var succeeded = _securityService.VerifyPasswordHash(request.Password, userData.Password, userData.Salt);

        if (!succeeded)
            return new LoginRespondDto { Success = false, MessageCode = "invalid_user_credentials" };

        if (userData.TwoFactorEnabled == true)
        {
            var otpCode = OtpService.GenerateOtpForUser(userData.Username);
            var otpSent = await OtpService.SendOtpAsync(otpCode, userData.Email ?? userData.TelePhone, (TwoFactorType)userData.TwoFactorType, _mailKitService, _configuration);

            if (!otpSent)
                return new LoginRespondDto { Success = false, MessageCode = "otp_send_failed" };

            var remainingSeconds = OtpService.GetOtpRemainingSeconds();
            return new LoginRespondDto
            {
                Success = false,
                MessageCode = "2fa_required",
                Message = $"OTP sent to your registered contact method. Valid for {remainingSeconds} seconds.",
                ExpiresInSeconds = remainingSeconds
            };
        }

        return await GenerateUserTokensAsync(userData);
    }

    public async Task<LoginRespondDto> VerifyOtpAsync(UserOtpVerificationDto request)
    {
        var userData = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.UserName);

        if (userData is null)
            return new LoginRespondDto { Success = false, MessageCode = "user_not_found" };

        var isValidOtp = OtpService.ValidateOtpForUser(request.OtpCode, userData.Username);

        if (!isValidOtp)
            return new LoginRespondDto { Success = false, MessageCode = "invalid_or_expired_otp", Message = "OTP is invalid or has expired." };

        return await GenerateUserTokensAsync(userData);
    }

    public async Task<bool> LogoutAsync(LogoutModel request)
    {
        var user = await _dbContext.Users.Where(x => x.Username == request.Username).FirstOrDefaultAsync();

        if (user == null) return false;

        user.IsOnline = false;
        _dbContext.Users.Update(user);
        var success = await _dbContext.SaveChangesAsync() > 0;

        if (success && !string.IsNullOrEmpty(request.RefreshToken))
        {
            await _dbContext.RefreshTokens.Where(rt => rt.UserId == user.Id && rt.Token == request.RefreshToken).ExecuteDeleteAsync();
        }

        return success;
    }

    public async Task<LoginRespondDto> RefreshTokenAsync(RefreshTokenModel request)
    {
        var userToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (userToken == null)
            return new LoginRespondDto { Success = false, MessageCode = "invalid_refersh_token" };

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userToken.UserId);

        if (user == null || user.IsLockedOut == true)
            return new LoginRespondDto { Success = false, MessageCode = "invalid_user_credentials" };

        var result = await GenerateUserTokensAsync(user);

        if (result.Success)
        {
            await _dbContext.RefreshTokens.Where(rt => rt.UserId == user.Id && rt.Token == request.RefreshToken).ExecuteDeleteAsync();
        }

        return result;
    }

    private async Task<LoginRespondDto> GenerateUserTokensAsync(User user)
    {
        var refreshToken = Guid.CreateVersion7().ToString("N");
        var newJwtToken = await TokenService.GenerateAsync(user, refreshToken, _dbContext, _configuration);

        if (string.IsNullOrWhiteSpace(newJwtToken))
            return new LoginRespondDto { Success = false, MessageCode = "token_generation_failed" };

        var tokenExpMin = _configuration.GetValue<int>("Authentication:AccessTokenExpirationMinutes");
        if (tokenExpMin <= 0) tokenExpMin = 1440;

        var tokenExpires = DateTime.UtcNow.AddMinutes(tokenExpMin);

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            IsRevoked = false,
            ExpiryDate = tokenExpires,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.RefreshTokens.AddAsync(newRefreshToken);

        user.LastLoginDate = DateTime.UtcNow;
        user.IsOnline = true;
        _dbContext.Users.Update(user);

        var success = await _dbContext.SaveChangesAsync() > 0;

        return new LoginRespondDto
        {
            Success = success,
            AccessToken = newJwtToken,
            RefreshToken = refreshToken,
            RequiresTwoFactor = user.TwoFactorEnabled,
            TokenType = "Bearer",
            MessageCode = "login_successfull"
        };
    }
}