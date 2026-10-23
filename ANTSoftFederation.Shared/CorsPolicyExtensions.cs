
namespace SorteFederation.Shared;

public static class CorsPolicyExtensions
{
    public static void ConfigureCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(opt => opt.AddPolicy("SortePolicy", builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                 //.AllowCredentials()
                 .SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
                ;
        }));
    }
}