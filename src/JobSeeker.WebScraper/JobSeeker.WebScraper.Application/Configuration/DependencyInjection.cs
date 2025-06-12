using Hangfire;
using Hangfire.PostgreSql;
using JobSeeker.WebScraper.Application.Hosted;
using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Application.Services.DataSeeder;
using JobSeeker.WebScraper.Application.Services.JobRunner;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory;
using JobSeeker.WebScraper.Application.Services.Proxy;
using JobSeeker.WebScraper.Application.Services.SearchResultsParsing;
using JobSeeker.WebScraper.Shared;
using JobSeeker.WebScraper.Shared.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobSeeker.WebScraper.Application.Configuration;

public static class DependencyInjection
{
    public static void ConfigureApplication(this IHostApplicationBuilder app)
    {
        ConfigureInfrastructure(app.Services, app.Configuration);
        AddServices(app.Services);
        AddJobs(app.Services);
        AddHosted(app.Services);
    }

    private static void ConfigureInfrastructure(IServiceCollection services, IConfiguration appConfiguration)
    {
        services.AddHangfire((provider, configuration) =>
        {
            var connectionString = appConfiguration.GetConnectionString(ConfigurationKeys.DatabaseConnectionString);
            configuration
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseFilter(new AutomaticRetryAttribute { Attempts = 0 })
                .UsePostgreSqlStorage(x => x.UseNpgsqlConnection(connectionString),
                    new PostgreSqlStorageOptions
                    {
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        PrepareSchemaIfNecessary = true,
                        SchemaName = "hangfire"
                    });
        });
        services.AddHangfireServer(options =>
        {
            options.Queues = [JobQueue.ScrapGroups, JobQueue.ScrapTasks];
            options.WorkerCount = 3;
        });
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<JobRunnerService>();
        services.AddSingleton<PlaywrightFactoryService>();
        services.AddSingleton<IProxyFactoryService, LocalProxyFactoryService>();
        services.AddSingleton<ISearchResultsParsingStrategyFactory, SearchResultsParsingStrategyFactory>();
        services.AddKeyedScoped<ISearchResultsParsingStrategy, HhSearchResultsParsingStrategy>(HhSearchResultsParsingStrategy.Domain);
        services.AddScoped<IDataSeeder, ScrapGroupSeeder>();
    }

    private static void AddJobs(IServiceCollection services)
    {
        var jobInterfaceType = typeof(IJob<>);

        var types = jobInterfaceType.Assembly.GetExportedTypes()
            .Where(t => t
                .GetInterfaces()
                .Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == jobInterfaceType
                ))
            .Select(t => new
            {
                Implementation = t,
                ServiceType = t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == jobInterfaceType).Take(2).ToList()
            });

        foreach (var type in types)
        {
            if (type.ServiceType.Count > 1) throw new Exception("Too many job interfaces");
            services.AddTransient(type.ServiceType[0], type.Implementation);
        }
    }

    private static void AddHosted(IServiceCollection services)
    {
        services.AddHostedService<DataSeedingHostedService>();
        services.AddHostedService<ScrapingScheduler>();
    }
}