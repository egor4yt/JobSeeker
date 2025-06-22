using Hangfire;
using Hangfire.MemoryStorage;
using JobSeeker.PagesAnalyzer.Application.Jobs.Base;
using JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy;
using JobSeeker.PagesAnalyzer.Application.Services.JobRunner;
using JobSeeker.PagesAnalyzer.Application.Services.Normalizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobSeeker.PagesAnalyzer.Application.Configuration;

public static class DependencyInjection
{
    public static void ConfigureApplication(this IHostApplicationBuilder app)
    {
        ConfigureInfrastructure(app.Services);
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
        services.AddSingleton<INormalizer, SimpleNormalizer>();
        services.AddSingleton<IAnalyzeStrategyFactory, AnalyzeStrategyFactory>();
        services.AddKeyedScoped<IAnalyzeStrategy, HhAnalyzeStrategy>(HhAnalyzeStrategy.Domain);
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

        services.AddSingleton<JobRunnerService>();
    }
}