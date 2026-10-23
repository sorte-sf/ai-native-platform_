
namespace Butterfly.Shared;

interface IScoped<T>
{
    T Service { get; }
}

public static class RegistrationExtensions
{
    public static IServiceCollection AddScopedAs<T>(this IServiceCollection services, IEnumerable<Type> types)
        where T : class
    {
        services.AddScoped<T>();

        foreach (var type in types)
        {
            services.AddScoped(type, svc =>
            {
                var rs = svc.GetRequiredService<T>();
                return rs;
            });
        }

        return services;
    }

    public class Scoped<T>(IServiceScopeFactory scopeFactory) : IScoped<T>,
      IDisposable where T : notnull
    {
        private readonly IServiceScope _scope = scopeFactory.CreateScope();

        public T Service => _scope.ServiceProvider.GetRequiredService<T>();

        public void Dispose()
        {
            _scope.Dispose();
        }

    }

    public static Dictionary<string, string> GetMimeTypes()
    {
        return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
    }

}

