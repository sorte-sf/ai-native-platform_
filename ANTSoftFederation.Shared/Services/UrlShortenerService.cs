using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed; 

namespace SorteFederation.Shared.Services;

public class UrlShortenerService
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IDistributedCache _cache;

    public UrlShortenerService(LinkGenerator linkGenerator, IDistributedCache cache)
    {
        _linkGenerator = linkGenerator;
        _cache = cache;
    }

    public async Task<string> GenerateShortUrl(HttpContext context, string controller, string action, object routeValues)
    {
        var longUrl = _linkGenerator.GetUriByAction(
            httpContext: context,
            action: action,
            controller: controller,
            values: routeValues
        );

        if (string.IsNullOrEmpty(longUrl))
        {
            throw new InvalidOperationException("Failed to generate URL.");
        }
         
        var shortCode = Guid.NewGuid().ToString("n").Substring(0, 6);
         
        await _cache.SetStringAsync(shortCode, longUrl, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
        });
         
        var shortUrl = $"{context.Request.Scheme}://{context.Request.Host}/short/{shortCode}";

        return shortUrl;
    }

    public async Task<string> ResolveOrRedirectUrl(string codeOrUrl)
    { 
        var longUrl = await _cache.GetStringAsync(codeOrUrl);
         
        if (string.IsNullOrEmpty(longUrl))
        { 
            return codeOrUrl;
        }
         
        return longUrl;
    }
}
