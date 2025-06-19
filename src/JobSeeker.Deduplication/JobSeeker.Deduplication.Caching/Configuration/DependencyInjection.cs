using JobSeeker.Deduplication.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;

namespace JobSeeker.Deduplication.Caching.Configuration;

public static class DependencyInjection
{
    public static void ConfigureCache(this IHostApplicationBuilder app)
    {
        ConfigureInfrastructure(app.Services, app.Configuration);
    }

    private static void ConfigureInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection(ConfigurationKeys.CacheConnectionString);
        if (string.IsNullOrWhiteSpace(connectionString.Value))
        {
            Log.Warning("Cache disabled: environment variable '{ConnectionString}' was null", ConfigurationKeys.CacheConnectionString);
            return;
        }

        var cache = configuration.GetSection(ConfigurationKeys.CacheBroker);
        switch (cache.Value)
        {
            case "redis":
                services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString.Value));
                break;
            default:
                Log.Warning("Cache disabled: unsupported broker '{CacheBroker}'", cache.Value);
                break;
        }
    }
}