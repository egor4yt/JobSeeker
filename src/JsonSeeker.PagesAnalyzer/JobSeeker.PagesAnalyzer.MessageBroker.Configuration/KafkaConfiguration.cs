using Confluent.Kafka;
using JobSeeker.PagesAnalyzer.MessageBroker.Consumers;
using MassTransit;

namespace JobSeeker.PagesAnalyzer.MessageBroker.Configuration;

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
        config.AddProducer<Messages.ScrapTask.Completed>("scrap-task-completed");
    }

    private static void ConfigureTopics(IRiderRegistrationContext context, IKafkaFactoryConfigurator config)
    {
        config.TopicEndpoint<Null, Messages.HealthCheck.Perform>("health-check-perform", "health-check-perform-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.HealthCheck.Perform>(context);
        });

        config.TopicEndpoint<Null, Messages.ScrapTask.Completed>("scrap-task-completed", "scrap-task-completed-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.ScrapTask.Completed>(context);
        });
    }
}