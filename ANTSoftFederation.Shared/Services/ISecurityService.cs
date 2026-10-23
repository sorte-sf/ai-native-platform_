
using System.Security.Cryptography;
using System.Text;

namespace SorteFederation.Shared.Services;

public interface ISecurityService
{
    string GetSha256Hash(string input);
    byte[] GetpasswordSalt();
    byte[] GetpasswordHash(byte[] storedSalt, string password);
    bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt);
}

public class SecurityService : ISecurityService
{
    public string GetSha256Hash(string input)
    {
        using var hashAlgorithm = new SHA256CryptoServiceProvider();
        var byteValue = Encoding.UTF8.GetBytes(input);
        var byteHash = hashAlgorithm.ComputeHash(byteValue);
        return Convert.ToBase64String(byteHash);
    }

    public byte[] GetpasswordSalt()
    {
        using (var hmac = new HMACSHA512())
        {
            return hmac.Key;
        }
    }

    public byte[] GetpasswordHash(byte[] storedSalt, string password)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
        {
            return hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        if (password == null) throw new ArgumentNullException("password");
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    public bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        if (password == null) throw new ArgumentNullException("password");
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
        if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
        if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

        using (var hmac = new HMACSHA512(storedSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != storedHash[i]) return false;
            }
        }

        return true;
    }

}
