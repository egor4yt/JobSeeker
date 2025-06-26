using Confluent.Kafka;
using JobSeeker.WebApi.MessageBroker.Messages.DeduplicatedVacancy;
using MassTransit;

namespace JobSeeker.WebApi.MessageBroker.Configuration;

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
            var consumersAssembly = typeof(Consumers.AssemblyRunner).Assembly;
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
    }

    private static void ConfigureTopics(IRiderRegistrationContext context, IKafkaFactoryConfigurator config)
    {
        config.TopicEndpoint<Null, Uploaded>("deduplicated-vacancy-uploaded", "deduplicated-vacancy-uploaded-group", e =>
        {
            e.CheckpointInterval = TimeSpan.FromSeconds(1);
            e.AutoOffsetReset = AutoOffsetReset.Earliest;
            e.ConfigureConsumer<Consumers.DeduplicatedVacancy.Uploaded>(context);
        });
    }
}