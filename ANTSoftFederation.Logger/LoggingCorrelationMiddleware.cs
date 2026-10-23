using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SorteFederation.Logger;

public class LoggingCorrelationMiddleware : IMiddleware
{
    private readonly ILogger<LoggingCorrelationMiddleware> _logger;

    public LoggingCorrelationMiddleware(ILogger<LoggingCorrelationMiddleware> logger)
    {
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var headers = Activity.Current.Baggage.ToDictionary(x => x.Key, x => x.Value);

        using (_logger.BeginScope(headers))
        {
            return next(context);
        }
    }
}