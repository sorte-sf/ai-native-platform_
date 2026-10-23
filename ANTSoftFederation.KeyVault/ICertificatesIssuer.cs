using System.Security.Cryptography.X509Certificates; 

namespace SorteFederation.KeyVault;


public interface ICertificatesIssuer
{
    Task<X509Certificate2> IssueAsync();
}