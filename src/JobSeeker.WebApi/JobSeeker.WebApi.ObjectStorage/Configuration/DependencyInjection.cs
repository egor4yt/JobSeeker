using Amazon.S3;
using JobSeeker.WebApi.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace JobSeeker.WebApi.ObjectStorage.Configuration;

public static class DependencyInjection
{
    public static void ConfigureObjectStorage(this IHostApplicationBuilder app)
    {
        ConfigureInfrastructure(app.Services, app.Configuration);
    }

    private static void ConfigureInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        var objectStorageBroker = configuration.GetSection(ConfigurationKeys.ObjectStorageBroker);
        switch (objectStorageBroker.Value)
        {
            case "minio":
                var connectionString = configuration.GetSection(ConfigurationKeys.ObjectStorageConnectionString);
                if (string.IsNullOrWhiteSpace(connectionString.Value))
                {
                    Log.Warning("Object storage broker disabled: environment variable '{ConnectionString}' was null", ConfigurationKeys.ObjectStorageConnectionString);
                    return;
                }

                var connectionStringParts = connectionString.Value.Split(';');
                if (connectionStringParts.Length != 3)
                {
                    Log.Warning("Object storage broker disabled: environment variable '{ConnectionString}' was incorrect formate (host;user;password)", ConfigurationKeys.ObjectStorageConnectionString);
                    return;
                }

                services.AddSingleton<IAmazonS3>(_ =>
                {
                    var config = new AmazonS3Config
                    {
                        ServiceURL = connectionStringParts[0],
                        ForcePathStyle = true
                    };
                    return new AmazonS3Client(connectionStringParts[1], connectionStringParts[2], config);
                });
                services.AddScoped<IObjectStorage, MinioStorage>();
                break;
            default:
                Log.Warning("Object storage broker disabled: unsupported broker '{ObjectStorageBroker}'", objectStorageBroker.Value);
                break;
        }
    }
}