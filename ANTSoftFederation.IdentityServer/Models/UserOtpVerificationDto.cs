namespace ANTSoftFederation.IdentityServer.Models;

public class UserOtpVerificationDto
{
    public string UserName { get; set; }
    public string OtpCode { get; set; }
}
