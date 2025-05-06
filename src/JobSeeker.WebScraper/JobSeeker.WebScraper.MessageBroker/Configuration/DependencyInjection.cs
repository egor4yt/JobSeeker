using System.Net;
using Confluent.Kafka;
using JobSeeker.WebScraper.MessageBroker.Providers.Base;
using JobSeeker.WebScraper.MessageBroker.Providers.Kafka;
using JobSeeker.WebScraper.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace JobSeeker.WebScraper.MessageBroker.Configuration;

public static class DependencyInjection
{
    /// <param name="builder">Web application instance reference</param>
    public static void ConfigureMessageBroker(this IHostApplicationBuilder builder)
    {
        ConfigureInfrastructure(builder.Services, builder.Configuration);
    }

    private static void ConfigureInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        var messageBroker = configuration.GetSection(ConfigurationKeys.MessageBroker);
        switch (messageBroker.Value)
        {
            case "kafka":
                var connectionString = configuration.GetSection(ConfigurationKeys.KafkaConnectionString);
                if (string.IsNullOrWhiteSpace(connectionString.Value))
                {
                    Log.Warning("Message broker disabled: environment variable '{ConnectionString}' was null", ConfigurationKeys.KafkaConnectionString);
                    break;
                }

                var producerConfig = new ProducerConfig
                {
                    BootstrapServers = connectionString.Value,
                    ClientId = Dns.GetHostName(),
                    AllowAutoCreateTopics = true,
                    Acks = Acks.All,
                    EnableIdempotence = true,
                    MaxInFlight = 5
                };
                services.AddSingleton(producerConfig);

                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = connectionString.Value,
                    GroupId = "web-scraper",
                    ClientId = Dns.GetHostName(),
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false,
                    EnableAutoOffsetStore = false
                };

                services.AddSingleton(consumerConfig);

                services.AddSingleton<IMessageProducer, KafkaMessageProducer>();
                services.AddHostedService<KafkaMessageConsumer>();
                break;
            default:
                Log.Warning("Message broker disabled: unsupported message broker '{MessageBroker}'", messageBroker.Value);
                break;
        }
    }
}