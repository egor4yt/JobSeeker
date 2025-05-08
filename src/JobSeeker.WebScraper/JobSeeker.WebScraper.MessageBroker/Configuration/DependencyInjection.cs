using JobSeeker.WebScraper.MessageBroker.Producers;
using JobSeeker.WebScraper.Shared;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace JobSeeker.WebScraper.MessageBroker.Configuration;

public static class DependencyInjection
{
    public static void ConfigureMessageBroker(this IHostApplicationBuilder builder)
    {
        ConfigureInfrastructure(builder.Services, builder.Configuration);
    }

    private static void ConfigureInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();

            var connectionString = configuration.GetSection(ConfigurationKeys.KafkaConnectionString);
            if (string.IsNullOrWhiteSpace(connectionString.Value))
            {
                Log.Warning("Message broker disabled: environment variable '{ConnectionString}' was null", ConfigurationKeys.KafkaConnectionString);
                return;
            }

            var messageBroker = configuration.GetSection(ConfigurationKeys.MessageBroker);
            switch (messageBroker.Value)
            {
                case "kafka":
                    configurator.UsingKafka(connectionString.Value);
                    services.AddScoped(typeof(IMessageProducer<>), typeof(KafkaMessageProducer<>));
                    break;
                default:
                    Log.Warning("Message broker disabled: unsupported message broker '{MessageBroker}'", messageBroker.Value);
                    break;
            }
        });
    }
}