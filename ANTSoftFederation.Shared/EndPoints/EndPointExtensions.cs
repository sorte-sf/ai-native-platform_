using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace SorteFederation.Shared.EndPoints;

public static class EndPointExtensions
{
    public static IServiceCollection AddEndPoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] endpointServiceDescriptors = assembly.DefinedTypes
                                                                 .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                                                                                type.IsAssignableTo(typeof(IAppEndPoints)))
                                                                 .Select(type => ServiceDescriptor.Transient(typeof(IAppEndPoints), type))
                                                                 .ToArray();
        services.TryAddEnumerable(endpointServiceDescriptors);
        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? route = null, params string[] excludePatterns)
    {
        IEnumerable<IAppEndPoints> endpoints = app.Services.GetRequiredService<IEnumerable<IAppEndPoints>>();

        if (excludePatterns.Length == 0)
        {
            excludePatterns = new[] { "auth", "public" };
        }

        var authorizedEndpoints = new List<IAppEndPoints>();
        var excludedEndpoints = new List<IAppEndPoints>();

        foreach (IAppEndPoints endpoint in endpoints)
        {
            string typeName = endpoint.GetType().Name.ToLowerInvariant();
            string typeNamespace = endpoint.GetType().Namespace?.ToLowerInvariant() ?? string.Empty;

            bool shouldExclude = excludePatterns.Any(pattern =>
                typeName.Contains(pattern.ToLowerInvariant()) ||
                typeNamespace.Contains(pattern.ToLowerInvariant()));

            if (shouldExclude)
            {
                excludedEndpoints.Add(endpoint);
            }
            else
            {
                authorizedEndpoints.Add(endpoint);
            }
        }

        IEndpointRouteBuilder baseBuilder = route is null ? app : route;
        foreach (IAppEndPoints endpoint in excludedEndpoints)
        {
            endpoint.MapEndpoint(baseBuilder);
        }

        IEndpointRouteBuilder authorizedBuilder;
        if (route is null)
        {
            authorizedBuilder = app.MapGroup(string.Empty).RequireAuthorization();
        }
        else
        {
            authorizedBuilder = route.MapGroup(string.Empty).RequireAuthorization();
        }

        foreach (IAppEndPoints endpoint in authorizedEndpoints)
        {
            endpoint.MapEndpoint(authorizedBuilder);
        }

        return app;
    }

    public static IApplicationBuilder WithExclusions(this WebApplication app, RouteGroupBuilder? route = null)
    {
        IEnumerable<IAppEndPoints> endpoints = app.Services.GetRequiredService<IEnumerable<IAppEndPoints>>();

        var authorizedEndpoints = endpoints.Where(e => e is not IAllowAnonymousEndpoint);
        var excludedEndpoints = endpoints.Where(e => e is IAllowAnonymousEndpoint);

        IEndpointRouteBuilder baseBuilder = route is null ? app : route;
        foreach (IAppEndPoints endpoint in excludedEndpoints)
        {
            endpoint.MapEndpoint(baseBuilder);
        }

        IEndpointRouteBuilder authorizedBuilder;
        if (route is null)
        {
            authorizedBuilder = app.MapGroup(string.Empty).RequireAuthorization();
        }
        else
        {
            authorizedBuilder = route.MapGroup(string.Empty).RequireAuthorization();
        }

        foreach (IAppEndPoints endpoint in authorizedEndpoints)
        {
            endpoint.MapEndpoint(authorizedBuilder);
        }

        return app;
    }

    public static IApplicationBuilder MapEndpointsWithoutAuth(this WebApplication app, RouteGroupBuilder? route = null)
    {
        IEnumerable<IAppEndPoints> endpoints = app.Services.GetRequiredService<IEnumerable<IAppEndPoints>>();
        IEndpointRouteBuilder builder = route is null ? app : route;

        foreach (IAppEndPoints endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    public interface IAllowAnonymousEndpoint
    {
    }

}
