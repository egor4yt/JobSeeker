using System.Reflection;
using Hangfire;
using Hangfire.MemoryStorage;
using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Application.Services.JobRunner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobSeeker.WebScraper.Application.Configuration;

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
    }

    private static void AddJobs(IServiceCollection services)
    {
        var types = Assembly.GetExecutingAssembly().GetExportedTypes()
            .Where(t => t
                .GetInterfaces()
                .Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IJob<>)
                ))
            .Select(t => new
            {
                Implementation = t,
                ServiceType = t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJob<>)).Take(2).ToList()
            });

        foreach (var type in types)
        {
            if (type.ServiceType.Count > 1) throw new Exception("Too many job interfaces");
            services.AddTransient(type.ServiceType[0], type.Implementation);
        }

        services.AddSingleton<JobRunnerService>();
    }
}