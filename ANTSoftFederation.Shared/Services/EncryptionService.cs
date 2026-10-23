
using System.Security.Cryptography;
using System.Text;

namespace SorteFederation.Shared.Services;

public class EncryptionService
{
    public string Decrypt(string secretKey, string cipherText)
    {
        try
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);

            // Extract IV from the beginning of the cipher text
            byte[] iv = new byte[16];
            byte[] cipher = new byte[16];

            Array.Copy(fullCipher, iv, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(secretKey);
            aesAlg.IV = iv;

            using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
            {
                using (var msDecrypt = new MemoryStream(cipher))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle exception as needed
            throw new Exception("An error occurred during decryption", ex);
        }
    }
}
