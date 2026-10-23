using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http; 

namespace SorteFederation.Shared;

public static class RateLimitExtensions
{
    private static readonly string Policy = "PerUserRatelimit";

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(Policy, context =>
            {
                // We always have a user name
                var username = context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                return RateLimitPartition.GetTokenBucketLimiter(username, key =>
                {
                    return new()
                    {
                        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                        AutoReplenishment = true,
                        TokenLimit = 100,
                        TokensPerPeriod = 100,
                        QueueLimit = 100,
                    };
                });
            });
        });
    }

    public static IEndpointConventionBuilder RequirePerUserRateLimit(this IEndpointConventionBuilder builder)
    {
        return builder.RequireRateLimiting(Policy);
    }
}


/*
public static class RateLimitExtensions
{
    private static readonly string Policy = "PerUserRatelimit";

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                    ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                    Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = problemDetailsFactory
                        .CreateProblemDetails(
                            context.HttpContext,
                            StatusCodes.Status429TooManyRequests,
                            "too_many_requests",
                            detail: $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds."
                        );

                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
                }
            };

            options.AddFixedWindowLimiter("fixed", cfg =>
            {
                cfg.PermitLimit = 5;
                cfg.Window = TimeSpan.FromMinutes(1);
            });

            options.AddPolicy(Policy, context =>
            {
                // Fix: Properly extract user ID from JWT token
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            context.User?.FindFirst("sub")?.Value ??
                            context.User?.Identity?.Name;

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return RateLimitPartition.GetTokenBucketLimiter(userId, _ =>
                        new()
                        {
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            AutoReplenishment = true,
                            TokenLimit = 5,
                            TokensPerPeriod = 2
                        });
                }

                return RateLimitPartition.GetFixedWindowLimiter("anonymous", _ =>
                    new()
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    });

            });

        });
    }

    public static IEndpointConventionBuilder RequirePerUserRateLimit(this IEndpointConventionBuilder builder)
    {
        return builder.RequireRateLimiting(Policy);
    }
}
*/
