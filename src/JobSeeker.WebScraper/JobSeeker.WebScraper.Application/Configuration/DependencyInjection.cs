using Hangfire;
using Hangfire.MemoryStorage;
using JobSeeker.WebScraper.Application.Commands.Base;
using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Application.Services.JobRunner;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory;
using JobSeeker.WebScraper.Application.Services.Proxy;
using JobSeeker.WebScraper.Application.Services.SearchResultsParsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobSeeker.WebScraper.Application.Configuration;

public static class DependencyInjection
{
    public static void ConfigureApplication(this IHostApplicationBuilder app)
    {
        ConfigureInfrastructure(app.Services);
        AddServices(app.Services);
        AddCommands(app.Services);
        AddJobs(app.Services);
    }

    private static void ConfigureInfrastructure(IServiceCollection services)
    {
        services.AddHangfire((provider, configuration) =>
        {
            configuration
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage();
        });
        services.AddHangfireServer();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<JobRunnerService>();
        services.AddSingleton<PlaywrightFactoryService>();
        services.AddSingleton<IProxyFactoryService, LocalProxyFactoryService>();
        services.AddSingleton<ISearchResultsParsingStrategyFactory, SearchResultsParsingStrategyFactory>();
        services.AddKeyedScoped<ISearchResultsParsingStrategy, HhSearchResultsParsingStrategy>(HhSearchResultsParsingStrategy.Domain);
    }

    private static void AddCommands(IServiceCollection services)
    {
        var commandHandlerType = typeof(ICommandHandler<>);

        var types = commandHandlerType.Assembly.GetExportedTypes()
            .Where(t => t
                .GetInterfaces()
                .Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == commandHandlerType
                ))
            .Select(t => new
            {
                Implementation = t,
                ServiceType = t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandHandlerType).Take(2).ToList()
            });

        foreach (var type in types)
        {
            if (type.ServiceType.Count > 1) throw new Exception("Too many command interfaces");
            services.AddTransient(type.ServiceType[0], type.Implementation);
        }
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
}