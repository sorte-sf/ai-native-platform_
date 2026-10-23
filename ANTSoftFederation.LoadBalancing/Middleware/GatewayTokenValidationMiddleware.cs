
namespace SorteFederation.LoadBalancing.Middleware;

/// <summary>
/// Enforces authentication at the gateway before a request can reach YARP.
/// Only the authentication entry point, health probes, and CORS preflight are public.
/// </summary>
public sealed class GatewayTokenValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicRequest(context))
        {
            await next(context);
            return;
        }

        var scheme = context.Request.Headers.Authorization.ToString().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)
            ? "GatewayBasic"
            : null;
        var result = await context.AuthenticateAsync(scheme);
        if (!result.Succeeded)
        {
            await context.ChallengeAsync();
            return;
        }

        context.User = result.Principal!;
        await next(context);
    }

    private static bool IsPublicRequest(HttpContext context)
    {
        if (HttpMethods.IsOptions(context.Request.Method)) return true;

        var path = context.Request.Path;
        if (path.StartsWithSegments("/health") || path.StartsWithSegments("/alive")) return true;

        return path.Equals("/auth", StringComparison.OrdinalIgnoreCase) ||
               path.Equals("/auth/login", StringComparison.OrdinalIgnoreCase) ||
               path.Equals("/auth/logout", StringComparison.OrdinalIgnoreCase);
    }
}
