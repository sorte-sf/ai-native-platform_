using ANTSoftFederation.IdentityServer.Services.Email;
using OtpNet;
using System.Security.Cryptography;
using System.Text;

namespace ANTSoftFederation.IdentityServer.Services.Auths;

public static class OtpService
{   
    public static string GenerateOtp(string secretKey, int stepSize = 180)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var totp = new Totp(keyBytes, step: stepSize, totpSize: 6);
        return totp.ComputeTotp();
    }

    public static string GenerateOtpForUser(string userIdentifier, int stepSize = 180)
    {
        var secretKey = GenerateSecretKeyFromUser(userIdentifier);
        return GenerateOtp(secretKey, stepSize);
    }

    public static bool ValidateOtp(string userOtp, string secretKey, int stepSize = 180, int window = 0)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var totp = new Totp(keyBytes, step: stepSize, totpSize: 6);
         
        if (totp.ComputeTotp() == userOtp)
        {
            return true;
        }

        return false;
    }

    public static bool ValidateOtpForUser(string userOtp, string userIdentifier, int stepSize = 180)
    {
        var secretKey = GenerateSecretKeyFromUser(userIdentifier);
        return ValidateOtp(userOtp, secretKey, stepSize, 0);
    }

    public static int GetOtpRemainingSeconds(int stepSize = 180)
    {
        var currentTimeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / stepSize;
        var nextTimeStep = (currentTimeStep + 1) * stepSize;
        var remainingSeconds = (int)(nextTimeStep - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        return remainingSeconds;
    }

    private static string GenerateSecretKeyFromUser(string userIdentifier)
    {
        var serverSecret = "TPSFedOtpSecretKey";
        var combined = $"{userIdentifier}:{serverSecret}";

        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));

            var secretBytes = new byte[20];
            Array.Copy(hash, secretBytes, 20);
            return Base32Encoding.ToString(secretBytes);
        }
    }

    public static async Task<bool> SendOtpAsync(string otpCode, string userContact, TwoFactorType method, IMailKitService mailKitService, IConfiguration configuration)
    {
        try
        {
            var success = false;
            var fromAddress = configuration.GetSection("app:EmailSettings:From").Value;

            if (method == TwoFactorType.SMS)
            {
                Console.WriteLine($"Sending SMS OTP {otpCode} to {userContact}");
                // await smsService.SendAsync(userContact, $"Your login code: {otpCode}");
            }

            if (method == TwoFactorType.Email)
            {
                Console.WriteLine($"Sending Email OTP {otpCode} to {userContact}");

                var mailData = new MailData(
                    to: userContact,
                    subject: "VipasX Login Code",
                    body: $"<p><b>Your VipasX login code:<h1>{otpCode}</h1></b></p>",
                    from: fromAddress
                  );

                success = await mailKitService.SendAsync(mailData, default);
            }

            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send OTP: {ex.Message}");
            return false;
        }
    }
}
