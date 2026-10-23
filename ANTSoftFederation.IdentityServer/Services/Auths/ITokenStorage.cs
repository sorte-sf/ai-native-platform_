using System.Collections.Concurrent;

namespace ANTSoftFederation.IdentityServer.Services.Auths;

public interface ITokenStorage
{
    void StoreToken(string userId, string token);
    string GetToken(string userId);
}

public class InMemoryTokenStore : ITokenStorage
{
    private readonly ConcurrentDictionary<string, string> _tokens = new ConcurrentDictionary<string, string>();

    public void StoreToken(string userId, string token)
    {
        _tokens[userId] = token;
    }

    public string GetToken(string userId)
    {
        _tokens.TryGetValue(userId, out var token);
        return token;
    }
}
