using System.Security.Cryptography;
using System.Text;

namespace SorteFederation.Shared.Helpers;

public static class KeyGenerator
{
    private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static (string publicKey, string secretKey) GenerateKeys()
    {
        using var rsa = RSA.Create(2048);
        string publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());

        string secretKey = Convert.ToBase64String( rsa.ExportPkcs8PrivateKey());

        return (publicKey, secretKey);
    } 
   
    public static string Generate64Key(int length = 64)
    {
        var result = new StringBuilder(length);
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] buffer = new byte[sizeof(uint)];
            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                result.Append(AllowedChars[(int)(num % (uint)AllowedChars.Length)]);
            }
        }
        return result.ToString();
    }

    public static (string publicKey, string secretKey) GenerateAppKeys()
    {
        return (Generate64Key(64), Generate64Key(64));
    }

}