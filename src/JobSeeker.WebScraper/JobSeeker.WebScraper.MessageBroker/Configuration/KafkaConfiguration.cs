using System.Reflection;
using Confluent.Kafka;
using MassTransit;

namespace JobSeeker.WebScraper.MessageBroker.Configuration;

/// <summary>
///     Masstransit Kafka configuration
/// </summary>
internal static class KafkaConfiguration
{
    public static void UsingKafka(this IBusRegistrationConfigurator configurator, string connectionString)
    {
        configurator.UsingInMemory();

        configurator.AddRider(rider =>
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            rider.AddConsumers(currentAssembly);
            ConfigureProducers(rider);

            rider.UsingKafka((context, kafkaConfig) =>
            {
                kafkaConfig.Host(connectionString);

                ConfigureTopics(context, kafkaConfig);
            });
        });
    }

    private static void ConfigureProducers(IRiderRegistrationConfigurator config)
    {
    }

    private static void ConfigureTopics(IRiderRegistrationContext context, IKafkaFactoryConfigurator config)
    {
        config.TopicEndpoint<Null, Messages.HealthCheck.Perform>("health-check-perform", "health-check-perform-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.HealthCheck.Perform>(context);
        });

        config.TopicEndpoint<Null, Messages.ScrapTask.Created>("scrap-task-created", "scrap-task-created-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.ScrapTask.Created>(context);
        });
    }
}