using System.Reflection;
using Confluent.Kafka;
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

            var messageBroker = configuration.GetSection(ConfigurationKeys.MessageBroker);
            switch (messageBroker.Value)
            {
                case "kafka":
                    var connectionString = configuration.GetSection(ConfigurationKeys.KafkaConnectionString);
                    if (string.IsNullOrWhiteSpace(connectionString.Value)) Log.Warning("Message broker disabled: environment variable '{ConnectionString}' was null", ConfigurationKeys.KafkaConnectionString);

                    configurator.UsingInMemory();

                    configurator.AddRider(rider =>
                    {
                        var currentAssembly = Assembly.GetExecutingAssembly();
                        rider.AddConsumers(currentAssembly);

                        rider.UsingKafka((context, kafkaConfig) =>
                        {
                            kafkaConfig.Host(connectionString.Value);

                            kafkaConfig.TopicEndpoint<Null, Messages.HealthCheck.Perform>("health-check-perform", "health-check-perform-group", e =>
                            {
                                e.CheckpointMessageCount = 5;
                                e.CheckpointInterval = TimeSpan.FromSeconds(1);
                                e.AutoOffsetReset = AutoOffsetReset.Earliest;
                                e.ConfigureConsumer<Consumers.HealthCheck.Perform>(context);
                            });
                        });
                    });

                    break;
                default:
                    Log.Warning("Message broker disabled: unsupported message broker '{MessageBroker}'", messageBroker.Value);
                    break;
            }
        });
    }
}