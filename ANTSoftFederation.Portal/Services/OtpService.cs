

using OtpNet;
using System.Text;

namespace SorteFederation.Portal.Services;

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

}

