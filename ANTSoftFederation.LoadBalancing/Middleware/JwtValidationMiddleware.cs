 
namespace SorteFederation.LoadBalancing.Middleware;

public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    { 
        if (context.Request.Path == "/")
        {
            await _next(context);
            return;
        }
         
        if (context.Request.Path.StartsWithSegments("/auth"))
        {
            await _next(context);
            return;
        }
         
        var result = await context.AuthenticateAsync();
        
        if (!result.Succeeded)
        {
            context.Response.StatusCode = 401;
             
            if (result.Failure?.Message?.Contains("expired", StringComparison.OrdinalIgnoreCase) == true)
            {
                await context.Response.WriteAsync("Unauthorized: Token has expired");
            }
            else
            {
                await context.Response.WriteAsync("Unauthorized: Invalid token");
            }
            return;
        }

        await _next(context);
    }
}