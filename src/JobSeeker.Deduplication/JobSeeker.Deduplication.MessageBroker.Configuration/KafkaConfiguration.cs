using Confluent.Kafka;
using JobSeeker.Deduplication.MessageBroker.Consumers;
using JobSeeker.Deduplication.MessageBroker.Consumers.RawVacancy;
using MassTransit;

namespace JobSeeker.Deduplication.MessageBroker.Configuration;

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
        config.AddProducer<Messages.RawVacancy.RawSaved>("raw-vacancy-saved");
        config.AddProducer<Messages.RawVacancy.Deduplicated>("raw-vacancy-deduplicated");
    }

    private static void ConfigureTopics(IRiderRegistrationContext context, IKafkaFactoryConfigurator config)
    {
        config.TopicEndpoint<Null, Messages.HealthCheck.Perform>("health-check-perform", "health-check-perform-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.HealthCheck.Perform>(context);
        });
        config.TopicEndpoint<Null, Messages.ScrapTask.Analyzed>("scrap-task-analyzed", "scrap-task-analyzed-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.ScrapTask.Analyzed>(context);
        });
        config.TopicEndpoint<Null, Messages.RawVacancy.RawSaved>("raw-vacancy-saved", "raw-vacancy-saved-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<RawSaved>(context);
        });
        config.TopicEndpoint<Null, Messages.RawVacancy.Deduplicated>("raw-vacancy-deduplicated", "raw-vacancy-deduplicated-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Deduplicated>(context);
        });
    }
}