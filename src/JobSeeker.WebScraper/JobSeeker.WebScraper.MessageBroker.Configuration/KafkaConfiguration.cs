using Confluent.Kafka;
using JobSeeker.WebScraper.MessageBroker.Consumers;
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
            var consumersAssembly = typeof(AssemblyRunner).Assembly;
            rider.AddConsumers(consumersAssembly);
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
        config.AddProducer<Messages.ScrapTaskResult.Created>("scrap-task-result-created");
        config.AddProducer<Messages.ScrapTask.Completed>("scrap-task-completed");
        config.AddProducer<Messages.ScrapTask.Created>("scrap-task-created");
    }

    private static void ConfigureTopics(IRiderRegistrationContext context, IKafkaFactoryConfigurator config)
    {
        config.TopicEndpoint<Null, Messages.HealthCheck.Perform>("health-check-perform", "health-check-perform-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.HealthCheck.Perform>(context);
        });

        config.TopicEndpoint<Null, Messages.ScrapTaskResult.Created>("scrap-task-result-created", "scrap-task-result-created-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.ScrapTaskResult.Created>(context);
        });

        config.TopicEndpoint<Null, Messages.ScrapTask.Created>("scrap-task-created", "scrap-task-created-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.ScrapTask.Created>(context);
        });
    }
}