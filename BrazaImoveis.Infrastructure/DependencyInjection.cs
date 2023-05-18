using BrazaImoveis.Infrastructure.Cache;
using BrazaImoveis.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrazaImoveis.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddMemoryCache()

            .AddScoped(_ =>
                new Supabase.Client(
                    configuration["SupabaseUrl"]!,
                    configuration["SupabaseKey"]!,
                    new Supabase.SupabaseOptions
                    {
                        AutoRefreshToken = true,
                        AutoConnectRealtime = true
                    }))

            .AddScoped<IDatabaseClient, SupabaseDatabaseClient>()
            .AddScoped<ISupabaseCachedClient, SupabaseCachedClient>()
            .Decorate<ISupabaseCachedClient, CacheDatabaseDecorator>();
    }
}
