using OtpNet;
using QRCoder;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ANTSoftFederation.IdentityServer.Services.Auths;

public class TwoFactorService
{
    private readonly ConcurrentDictionary<string, DateTime> _tokens = new();

    public TwoFactorSettings Generate2FASecret(string username)
    {
        var secretKey = GenerateSecretKey();
        var qrCodeUrl = GenerateQRCodeUrl(secretKey, username);
        return new TwoFactorSettings
        {
            SecretKey = secretKey,
            QRCodeUrl = qrCodeUrl
        };
    }

    public bool Validate2FAToken(string secretKey, string token)
    {
        var byteSecretKey = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(byteSecretKey);
        return totp.VerifyTotp(token, out long timeStepMatched);
    }

    public string GenerateOneTimeToken(int expirationInMinutes)
    {
        var token = GenerateUniqueToken();
        var expirationTime = DateTime.UtcNow.AddMinutes(expirationInMinutes);
        _tokens[token] = expirationTime;
        return token;
    }

    public bool ValidateOneTimeToken(string token)
    {
        if (_tokens.TryRemove(token, out var expirationTime))
        {
            return DateTime.UtcNow <= expirationTime;
        }
        return false;
    }

    private string GenerateSecretKey()
    {
        var secretKey = Base32Encoding.ToString(RandomNumberGenerator.GetBytes(10));
        return secretKey;
    }

    private string GenerateQRCodeUrl(string secretKey, string username)
    {
        var url = $"otpauth://totp/{username}?secret={secretKey}&issuer=teknopalas";
        using (var qrGenerator = new QRCodeGenerator())
        {
            using (var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                return Convert.ToBase64String(qrCode.GetGraphic(20));
            }
        }
    }

    private string GenerateUniqueToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[32]; // 256-bit token
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).TrimEnd('=');

    }
}

public class TwoFactorSettings
{
    public string SecretKey { get; set; }
    public string QRCodeUrl { get; set; }
}
