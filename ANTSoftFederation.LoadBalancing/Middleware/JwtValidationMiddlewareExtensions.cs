
namespace Microsoft.AspNetCore.Builder;

public static class JwtValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtValidationMiddleware>();
    }
}