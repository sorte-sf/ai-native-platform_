 
namespace ANTSoftFederation.IdentityServer.Services.Auths;

public interface IAuthService
{
    Task<LoginRespondDto> LoginAsync(LoginRequestDto request);
    Task<LoginRespondDto> VerifyOtpAsync(UserOtpVerificationDto request);
    Task<bool> LogoutAsync(LogoutModel request);
    Task<LoginRespondDto> RefreshTokenAsync(RefreshTokenModel request);
     

}
