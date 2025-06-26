using System.Reflection;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using JobSeeker.WebApi.Application.Behaviours;
using JobSeeker.WebApi.Application.Jobs.Base;
using JobSeeker.WebApi.Application.Services.JobRunner;
using JobSeeker.WebApi.Shared;
using JobSeeker.WebApi.Shared.Constants;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobSeeker.WebApi.Application.Configuration;

public static class DependencyInjection
{
    public static void ConfigureApplication(this IHostApplicationBuilder app)
    {
        ConfigureInfrastructure(app.Services, app.Configuration);
        AddServices(app.Services);
        AddJobs(app.Services);
    }

    private static void ConfigureInfrastructure(IServiceCollection services, IConfiguration appConfiguration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(config => { config.RegisterServicesFromAssembly(assembly); });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

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
                        SchemaName = "hangfire",
                        PrepareSchemaIfNecessary = true,
                        InvisibilityTimeout = TimeSpan.FromHours(3),
                        QueuePollInterval = TimeSpan.FromSeconds(15)
                    });
        });
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 3;
            options.ServerTimeout = TimeSpan.FromHours(3);
            options.Queues = [JobQueue.DownloadVacancies];
        });
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<JobRunnerService>();
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