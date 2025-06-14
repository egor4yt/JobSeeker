using Hangfire;
using Hangfire.MemoryStorage;
using JobSeeker.Deduplication.Application.Jobs.Base;
using JobSeeker.Deduplication.Application.Services.Fingerprints;
using JobSeeker.Deduplication.Application.Services.JobRunner;
using JobSeeker.Deduplication.Application.Services.Normalizer;
using JobSeeker.Deduplication.Application.Services.Tokenizer;
using JobSeeker.Deduplication.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobSeeker.Deduplication.Application.Configuration;

public static class DependencyInjection
{
    public static void ConfigureApplication(this IHostApplicationBuilder app)
    {
        ConfigureInfrastructure(app.Services, app.Configuration);
        AddServices(app.Services);
        AddJobs(app.Services);
    }

    private static void ConfigureInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire((provider, hangfireConfiguration) =>
        {
            hangfireConfiguration
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage();
        });
        services.AddHangfireServer();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<JobRunnerService>();
        services.AddScoped<INormalizer, SimpleNormalizer>();
        services.AddScoped<ITokenizer, SimpleTokenizer>();
        services.AddScoped(typeof(IFingerprintStrategy<RawVacancy>), typeof(RawVacancyFingerprintStrategy<RawVacancy>));
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