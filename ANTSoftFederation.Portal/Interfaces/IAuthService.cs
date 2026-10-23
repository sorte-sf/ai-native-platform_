namespace SorteFederation.Portal.Interfaces;

public interface IAuthService
{
    Task<LoginRespondDto> VerifyOtpAsync(UserOtpVerificationDto request);  
}
